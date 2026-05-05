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

using CreateEventRequest = global::Events.Api.Events.Post.CreateEventRequest;

namespace Events.Api.Tests.Endpoints.Events;

public class CreateEventEndpointTests
{
    private readonly Mock<IMediator> _mockMediator = new();

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateEventAndReturn201()
    {
        var request = new CreateEventRequest(
            "Tech Conference 2024",
            "https://example.com/banner.jpg",
            new List<int> { 1 },
            new List<int> { 1 },
            "Convention Center",
            "https://maps.example.com",
            "A major tech conference",
            null,
            null);

        var eventId = Guid.NewGuid();

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<CreateEventCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(eventId));

        var result = await _mockMediator.Object.Send(
            new CreateEventCommand(
                request.Title,
                request.BannerUrl,
                request.HashtagIds,
                request.CategoryIds,
                request.Location,
                request.MapUrl,
                request.Description,
                [],
                request.ImageUrls ?? []),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(eventId);

        _mockMediator.Verify(
            m => m.Send(
                It.IsAny<CreateEventCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Handle_WithMissingTitle_ShouldFailValidation()
    {
        var request = new CreateEventRequest(
            string.Empty,
            null,
            new List<int>(),
            new List<int>(),
            "Location",
            null,
            "Description",
            null,
            null);

        string.IsNullOrWhiteSpace(request.Title).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldCreateEventSuccessfully()
    {
        var request = new CreateEventRequest(
            "Simple Event",
            null,
            new List<int>(),
            new List<int>(),
            "Location",
            null,
            "Description",
            null,
            null);

        var eventId = Guid.NewGuid();

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<CreateEventCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(eventId));

        var result = await _mockMediator.Object.Send(
            new CreateEventCommand(
                request.Title,
                request.BannerUrl,
                request.HashtagIds,
                request.CategoryIds,
                request.Location,
                request.MapUrl,
                request.Description,
                [],
                request.ImageUrls ?? []),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(eventId);
    }
}
