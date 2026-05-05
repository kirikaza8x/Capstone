using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Events.Queries.GetEvents;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Xunit;

namespace Events.Application.Tests.Features.Events.Queries;

public class GetEventsQueryHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedEvents()
    {
        var query = new GetEventsQuery { PageNumber = 1, PageSize = 10 };
        var events = new List<EventResponse> { new() { Id = Guid.NewGuid(), Title = "Event 1" } };
        var pagedResult = PagedResult<EventResponse>.Create(events, 1, 10, 1);
        var expectedResult = Result.Success<PagedResult<EventResponse>>(pagedResult);
        SetupMediatorSendAny<GetEventsQuery, Result<PagedResult<EventResponse>>>(expectedResult);

        var result = await MockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNoResults_ShouldReturnEmptyPagedResult()
    {
        var query = new GetEventsQuery { PageNumber = 1, PageSize = 10, CategoryId = 999 };
        var pagedResult = PagedResult<EventResponse>.Create(new List<EventResponse>(), 1, 10, 0);
        var expectedResult = Result.Success<PagedResult<EventResponse>>(pagedResult);
        SetupMediatorSendAny<GetEventsQuery, Result<PagedResult<EventResponse>>>(expectedResult);

        var result = await MockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }
}
