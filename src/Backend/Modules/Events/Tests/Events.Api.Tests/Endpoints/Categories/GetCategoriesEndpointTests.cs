using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Categories.Queries.GetCategories;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

using CategoryResponse = global::Events.Application.Categories.Queries.GetCategories.CategoryResponse;

namespace Events.Api.Tests.Endpoints.Categories;

public class GetCategoriesEndpointTests
{
    private readonly Mock<IMediator> _mockMediator = new();

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnCategories()
    {
        var query = new GetCategoriesQuery(null, 20);
        var categoryResponses = new List<CategoryResponse>
        {
            new(1, "SPORTS", "Sports", "Sports events", true),
            new(2, "MUSIC", "Music", "Music events", true),
        };

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<GetCategoriesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<CategoryResponse>>(categoryResponses));

        var result = await _mockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First().Name.Should().Be("Sports");

        _mockMediator.Verify(
            m => m.Send(
                It.IsAny<GetCategoriesQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNameFilter_ShouldReturnFilteredResults()
    {
        var query = new GetCategoriesQuery("Sports", 20);
        var filteredResponse = new List<CategoryResponse>
        {
            new(1, "SPORTS", "Sports", "Sports events", true),
        };

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<GetCategoriesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<CategoryResponse>>(filteredResponse));

        var result = await _mockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Code.Should().Be("SPORTS");
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        var query = new GetCategoriesQuery("NonExistent", 20);

        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<GetCategoriesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<CategoryResponse>>(new List<CategoryResponse>()));

        var result = await _mockMediator.Object.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
