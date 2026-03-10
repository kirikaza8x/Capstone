using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Hashtags.Commands.DeleteHashtag;

public sealed record DeleteHashtagCommand(int HashtagId) : ICommand;