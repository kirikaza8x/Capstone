namespace AI.Application.Abstractions
{
    public interface IGeminiService
    {
        /// <summary>
        /// Free-form text generation (used sparingly, e.g. summaries, human-readable explanations).
        /// </summary>
        Task<string> GenerateTextAsync(
            string? systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Structured generation with strict JSON output.
        /// </summary>
        Task<TResponse> GenerateStructuredAsync<TResponse>(
            string? systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default
        ) where TResponse : class;

        /// <summary>
        /// Streaming chat responses, yielding plain string chunks.
        /// </summary>
        IAsyncEnumerable<string> StreamChatAsync(
            string userMessage,
            CancellationToken cancellationToken = default
        );
    }
}
