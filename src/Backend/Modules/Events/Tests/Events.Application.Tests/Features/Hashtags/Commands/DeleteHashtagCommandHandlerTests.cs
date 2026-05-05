using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Hashtags.Commands.DeleteHashtag;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;
using Xunit;

namespace Events.Application.Tests.Features.Hashtags.Commands;

public class DeleteHashtagCommandHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteSuccessfully()
    {
        var command = new DeleteHashtagCommand(1);
        var expectedResult = Result.Success();
        SetupMediatorSendAny<DeleteHashtagCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistent_ShouldReturnError()
    {
        var command = new DeleteHashtagCommand(999);
        var expectedResult = Result.Failure(HashtagErrors.NotFound(command.HashtagId));
        SetupMediatorSendAny<DeleteHashtagCommand, Result>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
