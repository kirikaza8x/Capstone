using System.Runtime.CompilerServices;

namespace AI.Application.Abstractions;

/// <summary>
/// Abstraction for Google Gemini AI operations.
/// Handles text generation, structured JSON output, and streaming responses.
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Generates free-form text from Gemini.
    /// </summary>
    /// <param name="userPrompt">The primary user instruction or query.</param>
    /// <param name="systemPromptOverride">
    /// Optional override for the system instruction configured at service initialization.
    /// Use sparingly—prefer setting the default system instruction via configuration.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The generated text response, or empty string if none.</returns>
    /// <remarks>
    /// The default system instruction is set via <c>GeminiConfig.SystemInstruction</c>.
    /// Only use <paramref name="systemPromptOverride"/> for per-request context shifts.
    /// </remarks>
    Task<string> GenerateTextAsync(
        string userPrompt,
        string? systemPromptOverride = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generates a strongly-typed response from Gemini using strict JSON mode.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The target type to deserialize the JSON response into.
    /// Must be a class with parameterless constructor and public settable properties.
    /// </typeparam>
    /// <param name="userPrompt">The user instruction containing the request context and data.</param>
    /// <param name="systemPromptOverride">
    /// Optional override for the system instruction. Use only when request-specific 
    /// system context is required beyond the configured default.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A deserialized instance of <typeparamref name="TResponse"/>.
    /// Throws <c>InvalidOperationException</c> if JSON parsing fails or response is empty.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when Gemini returns empty/invalid JSON or deserialization fails.
    /// </exception>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>Response JSON is automatically cleaned of markdown code blocks.</item>
    ///   <item>Property name matching is case-insensitive for resilience.</item>
    ///   <item>Trailing commas in JSON are tolerated to handle LLM quirks.</item>
    /// </list>
    /// </remarks>
    Task<TResponse> GenerateStructuredAsync<TResponse>(
        string userPrompt,
        string? systemPromptOverride = null,
        CancellationToken cancellationToken = default
    ) where TResponse : class;

    /// <summary>
    /// Streams chat responses from Gemini as plain text chunks.
    /// </summary>
    /// <param name="userMessage">The user message to send.</param>
    /// <param name="systemPromptOverride">
    /// Optional per-request system instruction override.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the streaming operation.</param>
    /// <returns>
    /// An async enumerable of text chunks. Consumers should iterate with <c>await foreach</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// await foreach (var chunk in gemini.StreamChatAsync("Explain quantum computing"))
    /// {
    ///     Console.Write(chunk); // Real-time output
    /// }
    /// </code>
    /// </example>
    IAsyncEnumerable<string> StreamChatAsync(
        string userMessage,
        string? systemPromptOverride = null,
        CancellationToken cancellationToken = default
    );
}
