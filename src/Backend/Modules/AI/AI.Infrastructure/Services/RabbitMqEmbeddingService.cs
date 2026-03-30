using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using AI.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Application.Abstractions.Embbeding;

namespace AI.Infrastructure.Embedding;

public sealed class RabbitMqEmbeddingService : IEmbeddingService, IAsyncDisposable
{
    private readonly IConnection _connection;
    private IChannel? _requestChannel;
    private IChannel? _responseChannel;
    private readonly EmbeddingQueueOptions _options;
    private readonly ILogger<RabbitMqEmbeddingService> _logger;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<float[]>> _pending = new();
    private readonly SemaphoreSlim _batchThrottle = new(10, 10);

    // 🔥 NEW: safe async init
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized = false;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqEmbeddingService(
        IConnection connection,
        IOptions<EmbeddingQueueOptions> options,
        ILogger<RabbitMqEmbeddingService> logger)
    {
        _connection = connection;
        _options    = options.Value;
        _logger     = logger;
    }

    // ─────────────────────────────────────────────────────────────
    // 🔧 SAFE LAZY INIT (NO BLOCKING IN DI)
    // ─────────────────────────────────────────────────────────────
    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized) return;

            _requestChannel  = await _connection.CreateChannelAsync(cancellationToken: ct);
            _responseChannel = await _connection.CreateChannelAsync(cancellationToken: ct);

            await _requestChannel.QueueDeclareAsync(
                _options.RequestQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);

            await _responseChannel.QueueDeclareAsync(
                _options.ResponseQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(_responseChannel);
            consumer.ReceivedAsync += OnResponseReceivedAsync;

            await _responseChannel.BasicConsumeAsync(
                _options.ResponseQueue,
                autoAck: true,
                consumer: consumer,
                cancellationToken: ct);

            _initialized = true;

            _logger.LogInformation(
                "RabbitMqEmbeddingService ready — request: {Req}, response: {Res}",
                _options.RequestQueue, _options.ResponseQueue);
        }
        finally
        {
            _initLock.Release();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // IEmbeddingService
    // ─────────────────────────────────────────────────────────────

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty.", nameof(text));

        await EnsureInitializedAsync(ct);

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
        if (texts.Count == 0)
            return Array.Empty<float[]>();

        var tasks = texts.Select(async text =>
        {
            await _batchThrottle.WaitAsync(ct);
            try
            {
                return await EmbedAsync(text, ct);
            }
            finally
            {
                _batchThrottle.Release();
            }
        });

        return await Task.WhenAll(tasks);
    }

    // ─────────────────────────────────────────────────────────────
    // INTERNAL PUBLISH
    // ─────────────────────────────────────────────────────────────

    private async Task PublishAsync(string correlationId, string text, CancellationToken ct)
    {
        var request = new EmbeddingRequested
        {
            CorrelationId = correlationId,
            Text          = text,
            Normalize     = true,
            RequestedAt   = DateTime.UtcNow.ToString("O")
        };

        var body = JsonSerializer.SerializeToUtf8Bytes(request, JsonOptions);

        var props = new BasicProperties
        {
            ContentType   = "application/json",
            DeliveryMode  = DeliveryModes.Persistent,
            MessageId     = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            ReplyTo       = _options.ResponseQueue, // 🔥 added
            Timestamp     = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await _requestChannel!.BasicPublishAsync(
            exchange: "",
            routingKey: _options.RequestQueue,
            mandatory: true,
            basicProperties: props,
            body: body,
            cancellationToken: ct);

        _logger.LogDebug("Published embedding request {CorrelationId}", correlationId);
    }

    // ─────────────────────────────────────────────────────────────
    // RESPONSE HANDLER
    // ─────────────────────────────────────────────────────────────

    private Task OnResponseReceivedAsync(object sender, BasicDeliverEventArgs e)
    {
        try
        {
            var correlationId = e.BasicProperties.CorrelationId;

            if (string.IsNullOrWhiteSpace(correlationId) ||
                !_pending.TryRemove(correlationId, out var tcs))
            {
                _logger.LogWarning(
                    "Received embedding response with unknown correlationId: {Id}",
                    correlationId);
                return Task.CompletedTask;
            }

            var json = Encoding.UTF8.GetString(e.Body.Span);

            var response = JsonSerializer.Deserialize<EmbeddingGenerated>(json, JsonOptions);

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

    // ─────────────────────────────────────────────────────────────
    // CLEANUP
    // ─────────────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        _batchThrottle.Dispose();
        _initLock.Dispose();

        if (_requestChannel  is not null) await _requestChannel.DisposeAsync();
        if (_responseChannel is not null) await _responseChannel.DisposeAsync();
    }
}

// ─────────────────────────────────────────────────────────────
// OPTIONS
// ─────────────────────────────────────────────────────────────

public sealed class EmbeddingQueueOptions
{
    public const string Section = "Embedding:Queue";

    public string RequestQueue  { get; set; } = "embedding.requests";
    public string ResponseQueue { get; set; } = "embedding.responses";
 
    public int TimeoutSeconds   { get; set; } = 30;
}