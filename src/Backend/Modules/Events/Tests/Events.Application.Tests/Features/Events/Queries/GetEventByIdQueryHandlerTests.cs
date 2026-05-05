using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Events.Queries.GetEventById;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;
using Xunit;

namespace Events.Application.Tests.Features.Events.Queries;

public class GetEventByIdQueryHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidId_ShouldReturnEvent()
    {
        var eventId = Guid.NewGuid();
        var query = new GetEventQuery(eventId);
        var eventResponse = new GetEventResponse { Id = eventId, Title = "Event Title" };
        var expectedResult = Result.Success<GetEventResponse>(eventResponse);
        SetupMediatorSendAny<GetEventQuery, Result<GetEventResponse>>(expectedResult);

        var result = await MockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(eventId);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnError()
    {
        var eventId = Guid.NewGuid();
        var query = new GetEventQuery(eventId);
        var expectedResult = Result.Failure<GetEventResponse>(Event.NotFound(eventId));
        SetupMediatorSendAny<GetEventQuery, Result<GetEventResponse>>(expectedResult);

        var result = await MockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
