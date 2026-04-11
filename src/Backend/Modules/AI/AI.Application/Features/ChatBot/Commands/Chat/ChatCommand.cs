
using Shared.Application.Abstractions.Messaging;

public record ChatCommand(
    string UserPrompt
) : ICommand<string>;

