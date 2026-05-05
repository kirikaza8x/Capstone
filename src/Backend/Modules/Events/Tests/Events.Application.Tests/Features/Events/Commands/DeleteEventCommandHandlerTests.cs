using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Events.Commands.DeleteEvent;
using FluentAssertions;
using MediatR;
using static Events.Domain.Errors.EventErrors;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

namespace Events.Application.Tests.Features.Events.Commands;

public class DeleteEventCommandHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteSuccessfully()
    {
        var eventId = Guid.NewGuid();
        var command = new DeleteEventCommand(eventId);
        var expectedResult = Result.Success();
        SetupMediatorSendAny<DeleteEventCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentEvent_ShouldReturnError()
    {
        var eventId = Guid.NewGuid();
        var command = new DeleteEventCommand(eventId);
        var expectedResult = Result.Failure(Event.NotFound(eventId));
        SetupMediatorSendAny<DeleteEventCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
