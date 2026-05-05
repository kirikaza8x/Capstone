using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Events.Commands.CreateEvent;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

namespace Events.Application.Tests.Features.Events.Commands;

public class CreateEventCommandHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEventSuccessfully()
    {
        var command = new CreateEventCommand(
            "Conference", 
            "https://example.com/banner.jpg",
            new List<int> { 1 },
            new List<int> { 1 },
            "Location",
            null,
            "Description",
            new List<CreateActorImageItem>(),
            new List<string> { "https://example.com/image.jpg" }
        );
        var expectedEventId = Guid.NewGuid();
        var expectedResult = Result.Success(expectedEventId);
        SetupMediatorSendAny<CreateEventCommand, Result<Guid>>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithNullTitle_ShouldReturnError()
    {
        var command = new CreateEventCommand(
            null!,
            "url",
            new List<int> { 1 },
            new List<int> { 1 },
            "Location",
            null,
            "Description",
            new List<CreateActorImageItem>(),
            new List<string> { "url" }
        );
        var expectedResult = Result.Failure<Guid>(Error.Validation("Event.Title.Required", "Title is required"));
        SetupMediatorSendAny<CreateEventCommand, Result<Guid>>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
