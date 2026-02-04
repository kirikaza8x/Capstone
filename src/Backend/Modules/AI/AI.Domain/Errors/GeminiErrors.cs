using Shared.Domain.Abstractions;

namespace AI.Domain.Gemini
{
    public static class GeminiErrors
    {
        public static Error ApiKeyMissing => Error.Validation(
            "Gemini.ApiKeyMissing",
            "Gemini API key is missing from configuration");

        public static Error EmptyPrompt => Error.Validation(
            "Gemini.EmptyPrompt",
            "Prompt cannot be empty");

        public static Error NullResponse => Error.Validation(
            "Gemini.NullResponse",
            "Gemini returned an empty or null response");

        public static Error InvalidJson(string rawResponse) => Error.Validation(
            "Gemini.InvalidJson",
            $"Gemini returned invalid JSON: {rawResponse}");

        public static Error Unexpected(string message) => Error.Validation(
            "Gemini.Unexpected",
            $"An unexpected error occurred while calling Gemini: {message}");
    }
}
