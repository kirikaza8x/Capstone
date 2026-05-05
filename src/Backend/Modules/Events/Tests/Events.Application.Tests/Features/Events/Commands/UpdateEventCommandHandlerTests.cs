using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Events.Commands.UpdateEvent;
using FluentAssertions;
using MediatR;
using static Events.Domain.Errors.EventErrors;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

namespace Events.Application.Tests.Features.Events.Commands;

public class UpdateEventCommandHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateSuccessfully()
    {
        var eventId = Guid.NewGuid();
        var command = new UpdateEventCommand(
            eventId,
            "Updated Event",
            null,
            null,
            "Location",
            null,
            "Description",
            null
        );
        var expectedResult = Result.Success();
        SetupMediatorSendAny<UpdateEventCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentEvent_ShouldReturnError()
    {
        var eventId = Guid.NewGuid();
        var command = new UpdateEventCommand(
            eventId,
            "Title",
            null,
            null,
            "Location",
            null,
            "Description",
            null
        );
        var expectedResult = Result.Failure(Event.NotFound(eventId));
        SetupMediatorSendAny<UpdateEventCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
