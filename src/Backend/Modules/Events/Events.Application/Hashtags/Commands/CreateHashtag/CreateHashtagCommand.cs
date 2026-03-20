using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Hashtags.Commands.CreateHashtag;

public sealed record CreateHashtagCommand(string Name) : ICommand<int>;
