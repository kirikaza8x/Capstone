using AI.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Application.Abstractions.Embbeding;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace AI.Infrastructure.Embedding;

/// <summary>
/// Sends text to the Python embedding service via RabbitMQ and waits for the response.
///
/// PATTERN: Request/response over two queues (matches Python RabbitMQConsumer exactly).
///   → Publish EmbeddingRequested  → REQUEST_QUEUE  → Python processes
///   → Receive EmbeddingGenerated  ← RESPONSE_QUEUE ← Python responds
///   → Matched by correlationId via ConcurrentDictionary of TaskCompletionSource
///
/// V7 CHANGES from v6:
///   - IModel       → IChannel
///   - CreateModel()→ await CreateChannelAsync()
///   - EventingBasicConsumer → AsyncEventingBasicConsumer with ReceivedAsync
///   - BasicPublish → await BasicPublishAsync
///   - QueueDeclare → await QueueDeclareAsync
///   - CreateBasicProperties() removed → use new BasicProperties()
/// </summary>
public sealed class RabbitMqEmbeddingService : IEmbeddingService, IAsyncDisposable
{
    private readonly IConnection _connection;
    private IChannel? _requestChannel;
    private IChannel? _responseChannel;
    private readonly EmbeddingQueueOptions _options;
    private readonly ILogger<RabbitMqEmbeddingService> _logger;

    // correlationId → TaskCompletionSource waiting for Python's response
    private readonly ConcurrentDictionary<string, TaskCompletionSource<float[]>> _pending = new();

    private RabbitMqEmbeddingService(
        IConnection connection,
        EmbeddingQueueOptions options,
        ILogger<RabbitMqEmbeddingService> logger)
    {
        _connection = connection;
        _options    = options;
        _logger     = logger;
    }

    /// <summary>
    /// Factory method — channels must be created async so constructor can't do it.
    /// Call this from DI registration instead of new().
    /// </summary>
    public static async Task<RabbitMqEmbeddingService> CreateAsync(
        IConnection connection,
        IOptions<EmbeddingQueueOptions> options,
        ILogger<RabbitMqEmbeddingService> logger,
        CancellationToken ct = default)
    {
        var service = new RabbitMqEmbeddingService(connection, options.Value, logger);
        await service.InitializeAsync(ct);
        return service;
    }

    private async Task InitializeAsync(CancellationToken ct)
    {
        // v7: CreateChannelAsync instead of CreateModel()
        _requestChannel  = await _connection.CreateChannelAsync(cancellationToken: ct);
        _responseChannel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await _requestChannel.QueueDeclareAsync(
            _options.RequestQueue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: ct);

        await _responseChannel.QueueDeclareAsync(
            _options.ResponseQueue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: ct);

        // v7: AsyncEventingBasicConsumer with ReceivedAsync (not Received)
        var consumer = new AsyncEventingBasicConsumer(_responseChannel);
        consumer.ReceivedAsync += OnResponseReceivedAsync;

        await _responseChannel.BasicConsumeAsync(
            _options.ResponseQueue, autoAck: true, consumer: consumer,
            cancellationToken: ct);

        _logger.LogInformation(
            "RabbitMqEmbeddingService ready — request: {Req}, response: {Res}",
            _options.RequestQueue, _options.ResponseQueue);
    }

    // ── IEmbeddingService ─────────────────────────────────────────

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty.", nameof(text));

        // Trim to 512 chars — matches Python validator max_length
        var trimmed = text.Length > 512 ? text[..512] : text;

        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<float[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[correlationId] = tcs;

        try
        {
            await PublishAsync(correlationId, trimmed, ct);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            await using var _ = cts.Token.Register(() =>
            {
                if (_pending.TryRemove(correlationId, out var t))
                    t.TrySetCanceled();
            });

            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            _pending.TryRemove(correlationId, out _);
            _logger.LogError(
                "Embedding request {CorrelationId} timed out after {Timeout}s",
                correlationId, _options.TimeoutSeconds);
            throw;
        }
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken ct = default)
    {
        var tasks = texts.Select(t => EmbedAsync(t, ct));
        return await Task.WhenAll(tasks);
    }


    private async Task PublishAsync(string correlationId, string text, CancellationToken ct)
    {
        var request = new EmbeddingRequested
        {
            CorrelationId = correlationId,
            Text          = text,
            Normalize     = true,
            RequestedAt   = DateTime.UtcNow.ToString("O")
        };

        var body = JsonSerializer.SerializeToUtf8Bytes(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var props = new BasicProperties
        {
            ContentType  = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId    = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            Timestamp    = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await _requestChannel!.BasicPublishAsync(
            exchange:   "",
            routingKey: _options.RequestQueue,
            mandatory:  true,
            basicProperties: props,
            body: body,
            cancellationToken: ct);

        _logger.LogDebug("Published embedding request {CorrelationId}", correlationId);
    }

    // v7: handler is async Task, not void
    private Task OnResponseReceivedAsync(object sender, BasicDeliverEventArgs e)
    {
        try
        {
            var correlationId = e.BasicProperties.CorrelationId;

            if (string.IsNullOrWhiteSpace(correlationId) ||
                !_pending.TryRemove(correlationId, out var tcs))
            {
                _logger.LogWarning(
                    "Received embedding response with unknown correlationId: {Id}", correlationId);
                return Task.CompletedTask;
            }

            // v7: body is ReadOnlyMemory<byte> — must copy before async use
            var json = Encoding.UTF8.GetString(e.Body.Span);

            var response = JsonSerializer.Deserialize<EmbeddingGenerated>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (response is null)
            {
                tcs.TrySetException(new InvalidOperationException("Null response from embedding service."));
                return Task.CompletedTask;
            }

            if (!response.Success || response.Embedding is null)
            {
                tcs.TrySetException(new InvalidOperationException(
                    $"Embedding service error: {response.Error}"));
                return Task.CompletedTask;
            }

            tcs.TrySetResult(response.Embedding.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process embedding response");
        }

        return Task.CompletedTask;
    }

    // v7: IAsyncDisposable instead of IDisposable
    public async ValueTask DisposeAsync()
    {
        if (_requestChannel is not null)  await _requestChannel.DisposeAsync();
        if (_responseChannel is not null) await _responseChannel.DisposeAsync();
    }
}

/// <summary>
/// Config bound from appsettings — queue names must match Python config.py exactly.
/// </summary>
public sealed class EmbeddingQueueOptions
{
    public const string Section = "Embedding:Queue";

    /// <summary>Must match Python REQUEST_QUEUE.</summary>
    public string RequestQueue  { get; set; } = "embedding.requests";

    /// <summary>Must match Python RESPONSE_QUEUE.</summary>
    public string ResponseQueue { get; set; } = "embedding.responses";

    /// <summary>Seconds to wait for Python response before timing out.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}