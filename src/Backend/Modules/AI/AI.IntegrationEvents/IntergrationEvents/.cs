namespace AI.IntegrationEvents;
/// <summary>
/// Sent to Python embedding service via RabbitMQ REQUEST_QUEUE.
/// Must match Python's EmbeddingRequested pydantic model exactly.
/// </summary>
public sealed record EmbeddingRequested
{
    /// <summary>Matched to response — use a fresh Guid per request.</summary>
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Text to embed. Max 512 chars — matches Python validator.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Whether to L2-normalize the output vector. Always true — matches Qdrant cosine setup.</summary>
    public bool Normalize { get; init; } = true;

    public string RequestedAt { get; init; } = DateTime.UtcNow.ToString("O");
}

/// <summary>
/// Received from Python embedding service via RabbitMQ RESPONSE_QUEUE.
/// Must match Python's EmbeddingGenerated pydantic model exactly.
/// </summary>
public sealed record EmbeddingGenerated
{
    public string CorrelationId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public List<float>? Embedding { get; init; }
    public int Dimension { get; init; } = 384;
    public string Model { get; init; } = string.Empty;
    public string? Error { get; init; }
    public string ProcessedAt { get; init; } = string.Empty;
}