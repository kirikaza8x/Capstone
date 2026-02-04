using AI.Application.Abstractions;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.ChatBot.Commands
{
    public class ChatCommandHandler : ICommandHandler<ChatCommand, string>
    {
        private readonly IGeminiService _geminiService;

        public ChatCommandHandler(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        public async Task<Result<string>> Handle(
            ChatCommand request,
            CancellationToken cancellationToken)
        {
            var systemPrompt = string.Empty; 
            var userPrompt = request.UserPrompt;

            var response = await _geminiService.GenerateTextAsync(
                systemPrompt,
                userPrompt,
                cancellationToken
            );

            if (string.IsNullOrWhiteSpace(response))
            {
                return Result.Failure<string>(
                    Error.Failure("Gemini.EmptyResponse", "Gemini returned an empty response.")
                );
            }

            return Result.Success(response);
        }
    }
}
