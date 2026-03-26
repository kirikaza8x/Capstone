using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventMembers.Queries.ExportEventMembers;

public record ExportEventMembersQuery(Guid EventId) : IQuery<byte[]>;
