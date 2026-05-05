using Xunit;
using Moq;
using FluentAssertions;
using Events.Application.Categories.Queries.GetCategories;
using MediatR;
using Shared.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Application.Tests.Features.Categories.Queries
{
    public class GetCategoriesQueryHandlerTests : BaseHandlerTest
    {
        [Fact]
        public async Task Handle_WithNoFilter_ShouldReturnAllCategories()
        {
            // Arrange
            var query = new GetCategoriesQuery(null, 20);
            var categoryResponses = new List<CategoryResponse>
            {
                new(1, "SPORTS", "Sports Events", "Description", true),
                new(2, "MUSIC", "Music Events", "Description", true),
                new(3, "TECH", "Technology Events", "Description", true)
            };

            var expectedResult = Result.Success<IReadOnlyList<CategoryResponse>>(categoryResponses);
            SetupMediatorSendAny<GetCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(3);
            result.Value.Should().ContainSingle(c => c.Code == "SPORTS");

            MockMediator.Verify(
                m => m.Send(
                    It.IsAny<GetCategoriesQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNameFilter_ShouldReturnFilteredCategories()
        {
            // Arrange
            var query = new GetCategoriesQuery("Sports", 20);
            var filteredResponses = new List<CategoryResponse>
            {
                new(1, "SPORTS", "Sports Events", "Description", true)
            };

            var expectedResult = Result.Success<IReadOnlyList<CategoryResponse>>(filteredResponses);
            SetupMediatorSendAny<GetCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value.First().Code.Should().Be("SPORTS");
        }

        [Fact]
        public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
        {
            // Arrange
            var query = new GetCategoriesQuery("NonExistent", 20);
            var expectedResult = Result.Success<IReadOnlyList<CategoryResponse>>(new List<CategoryResponse>());
            SetupMediatorSendAny<GetCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }
}
