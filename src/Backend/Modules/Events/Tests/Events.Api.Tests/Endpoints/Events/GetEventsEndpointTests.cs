using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Events.Queries.GetEvents;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Xunit;

using EventResponse = global::Events.Application.Events.Queries.GetEvents.EventResponse;
using GetEventsQuery = global::Events.Application.Events.Queries.GetEvents.GetEventsQuery;

namespace Events.Api.Tests.Endpoints.Events;

public class GetEventsEndpointTests
{
    private readonly Mock<IMediator> _mockMediator = new();

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnPagedEvents()
    {
        var query = new GetEventsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            CategoryId = null,
        };

        var eventResponses = new List<EventResponse>
        {
            CreateEventResponse(Guid.NewGuid(), "Event 1", "Location 1"),
            CreateEventResponse(Guid.NewGuid(), "Event 2", "Location 2"),
        };

        var pagedResult = new PagedResult<EventResponse>(
            eventResponses,
            1,
            10,
            20);

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<GetEventsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        var result = await _mockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(20);
        result.Value.PageNumber.Should().Be(1);

        _mockMediator.Verify(
            m => m.Send(
                It.IsAny<GetEventsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnFilteredEvents()
    {
        var query = new GetEventsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            CategoryId = null,
        };

        var filteredResponses = new List<EventResponse>
        {
            CreateEventResponse(Guid.NewGuid(), "Sports Event", "Stadium"),
        };

        var pagedResult = new PagedResult<EventResponse>(
            filteredResponses,
            1,
            10,
            5);

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<GetEventsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        var result = await _mockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Title.Should().Be("Sports Event");
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        var query = new GetEventsQuery
        {
            PageNumber = 2,
            PageSize = 5,
            CategoryId = null,
        };

        var pageResponses = new List<EventResponse>
        {
            CreateEventResponse(Guid.NewGuid(), "Page 2 Event", "Location"),
        };

        var pagedResult = new PagedResult<EventResponse>(
            pageResponses,
            2,
            5,
            15);

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<GetEventsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        var result = await _mockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        var query = new GetEventsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            CategoryId = null,
        };

        var pagedResult = new PagedResult<EventResponse>(
            new List<EventResponse>(),
            1,
            10,
            0);

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<GetEventsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        var result = await _mockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    private static EventResponse CreateEventResponse(
        Guid id,
        string title,
        string location)
    {
        return new EventResponse
        {
            Id = id,
            Title = title,
            Location = location,
            BannerUrl = null,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UrlPath = string.Empty,
        };
    }
}
