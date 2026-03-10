
using Shared.Application.Abstractions.Messaging;

public record BindGoogleCommand(string IdToken) : ICommand;

