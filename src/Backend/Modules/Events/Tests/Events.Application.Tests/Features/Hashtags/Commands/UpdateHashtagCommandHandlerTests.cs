using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Hashtags.Commands.UpdateHashtag;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;
using Xunit;

namespace Events.Application.Tests.Features.Hashtags.Commands;

public class UpdateHashtagCommandHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateSuccessfully()
    {
        var command = new UpdateHashtagCommand(1, "UpdatedName");
        var expectedResult = Result.Success();
        SetupMediatorSendAny<UpdateHashtagCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistent_ShouldReturnError()
    {
        var command = new UpdateHashtagCommand(999, "Name");
        var expectedResult = Result.Failure(HashtagErrors.NotFound(command.HashtagId));
        SetupMediatorSendAny<UpdateHashtagCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
