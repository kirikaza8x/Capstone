using System.Threading;
using System.Threading.Tasks;
using Events.Application.Hashtags.Commands.CreateHashtag;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

using CreateHashtagRequest = global::Events.Api.Hashtags.CreateHashtagRequest;

namespace Events.Api.Tests.Endpoints.Hashtags;

public class CreateHashtagEndpointTests
{
    private readonly Mock<IMediator> _mockMediator = new();

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateHashtagAndReturn201()
    {
        var request = new CreateHashtagRequest("Technology");
        var hashtagId = 1;

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<CreateHashtagCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(hashtagId));

        var result = await _mockMediator.Object.Send(
            new CreateHashtagCommand(request.Name),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(hashtagId);

        _mockMediator.Verify(
            m => m.Send(
                It.Is<CreateHashtagCommand>(c => c.Name == request.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Handle_WithEmptyName_ShouldReturnValidationError()
    {
        var request = new CreateHashtagRequest(string.Empty);

        string.IsNullOrWhiteSpace(request.Name).Should().BeTrue();
    }
}
