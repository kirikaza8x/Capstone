using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Events.Commands.PublishEvent;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;
using Xunit;

namespace Events.Application.Tests.Features.Events.Commands;

public class PublishEventCommandHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldPublishSuccessfully()
    {
        var eventId = Guid.NewGuid();
        var command = new PublishEventCommand(eventId);
        var expectedResult = Result.Success();
        SetupMediatorSendAny<PublishEventCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentEvent_ShouldReturnError()
    {
        var eventId = Guid.NewGuid();
        var command = new PublishEventCommand(eventId);
        var expectedResult = Result.Failure(Event.NotFound(eventId));
        SetupMediatorSendAny<PublishEventCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
