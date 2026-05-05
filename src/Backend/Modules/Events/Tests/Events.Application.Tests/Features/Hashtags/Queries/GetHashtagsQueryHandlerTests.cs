using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Hashtags.Queries.GetHashtags;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

namespace Events.Application.Tests.Features.Hashtags.Queries;

public class GetHashtagsQueryHandlerTests : BaseHandlerTest
{
    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnHashtags()
    {
        var query = new GetHashtagsQuery();
        var hashtags = new List<HashtagResponse>(new[] { new HashtagResponse(1, "Gaming", "gaming", 0) });
        var expectedResult = Result.Success<IReadOnlyList<HashtagResponse>>(hashtags.AsReadOnly());
        SetupMediatorSendAny<GetHashtagsQuery, Result<IReadOnlyList<HashtagResponse>>>(expectedResult);

        var result = await MockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithEmptyDatabase_ShouldReturnEmpty()
    {
        var query = new GetHashtagsQuery();
        var expectedResult = Result.Success<IReadOnlyList<HashtagResponse>>(new List<HashtagResponse>().AsReadOnly());
        SetupMediatorSendAny<GetHashtagsQuery, Result<IReadOnlyList<HashtagResponse>>>(expectedResult);

        var result = await MockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
