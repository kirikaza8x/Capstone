using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Hashtags.Commands.CreateHashtag;
using Events.Application.Hashtags.Queries.GetHashtags;
using FluentAssertions;
using static Events.Domain.Errors.EventErrors;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

namespace Events.Application.Tests.Features.Hashtags.Commands;

public class CreateHashtagCommandHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateHashtagSuccessfully()
    {
        var command = new CreateHashtagCommand("Gaming");
        var expectedResult = Result.Success(1);
        SetupMediatorSendAny<CreateHashtagCommand, Result<int>>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithDuplicateHashtag_ShouldReturnError()
    {
        var command = new CreateHashtagCommand("Music");
        var expectedResult = Result.Failure<int>(HashtagErrors.SlugAlreadyExists(command.Name ?? string.Empty));
        SetupMediatorSendAny<CreateHashtagCommand, Result<int>>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNullName_ShouldReturnValidationError()
    {
        var command = new CreateHashtagCommand(null!);
        var expectedResult = Result.Failure<int>(Shared.Domain.Abstractions.Error.Validation("Hashtag.Name.Required", "Name is required"));
        SetupMediatorSendAny<CreateHashtagCommand, Result<int>>(expectedResult);

        var result = await MockMediator.Object.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
