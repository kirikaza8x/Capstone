using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Hashtags.Commands.UpdateHashtag;

public sealed record UpdateHashtagCommand(int HashtagId, string Name, string Slug) : ICommand;