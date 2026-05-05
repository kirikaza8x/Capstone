using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Hashtags.Queries.GetHashtagById;
using Events.Application.Hashtags.Queries.GetHashtags;
using FluentAssertions;
using static Events.Domain.Errors.EventErrors;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

namespace Events.Application.Tests.Features.Hashtags.Queries;

public class GetHashtagByIdQueryHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidId_ShouldReturnHashtag()
    {
        var query = new GetHashtagByIdQuery(1);
        var hashtag = new HashtagResponse(1, "Gaming", "gaming", 0);
        var expectedResult = Result.Success<HashtagResponse>(hashtag);
        SetupMediatorSendAny<GetHashtagByIdQuery, Result<HashtagResponse>>(expectedResult);

        var result = await MockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnError()
    {
        var query = new GetHashtagByIdQuery(999);
        var expectedResult = Result.Failure<HashtagResponse>(HashtagErrors.NotFound(query.HashtagId));
        SetupMediatorSendAny<GetHashtagByIdQuery, Result<HashtagResponse>>(expectedResult);

        var result = await MockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
