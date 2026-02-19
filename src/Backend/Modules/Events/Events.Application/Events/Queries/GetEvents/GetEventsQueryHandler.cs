using AutoMapper;
using Events.Domain.Repositories;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetEvents;

internal sealed class GetEventsQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper) : IQueryHandler<GetEventsQuery, GetEventsResponse>
{
    public Task<Result<GetEventsResponse>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {

        throw new NotImplementedException();
    }
}
