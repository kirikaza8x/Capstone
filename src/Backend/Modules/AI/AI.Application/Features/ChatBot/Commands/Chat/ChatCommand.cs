
using Shared.Application.Messaging;

public record ChatCommand(
    string UserPrompt
): ICommand<string>;

