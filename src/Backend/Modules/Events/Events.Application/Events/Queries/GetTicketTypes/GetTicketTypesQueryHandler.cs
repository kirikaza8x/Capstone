using AutoMapper;
using Events.Application.Events.DTOs;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetTicketTypes;

internal sealed class GetTicketTypesQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper) : IQueryHandler<GetTicketTypesQuery, IReadOnlyList<TicketTypeDto>>
{
    public async Task<Result<IReadOnlyList<TicketTypeDto>>> Handle(
        GetTicketTypesQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithTicketTypesAsync(query.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<IReadOnlyList<TicketTypeDto>>(EventErrors.Event.NotFound(query.EventId));

        var response = mapper.Map<IReadOnlyList<TicketTypeDto>>(@event.TicketTypes);

        return Result.Success(response);
    }
}