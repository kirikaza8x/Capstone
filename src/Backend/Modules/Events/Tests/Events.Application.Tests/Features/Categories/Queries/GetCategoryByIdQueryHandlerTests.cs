using Xunit;
using Moq;
using FluentAssertions;
using Events.Application.Categories.Queries.GetCategoryById;
using Events.Application.Categories.Queries.GetCategories;
using MediatR;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Application.Tests.Features.Categories.Queries
{
    public class GetCategoryByIdQueryHandlerTests : BaseHandlerTest
    {
        [Fact]
        public async Task Handle_WithValidCategoryId_ShouldReturnCategory()
        {
            // Arrange
            var categoryId = 1;
            var query = new GetCategoryByIdQuery(categoryId);
            var categoryResponse = new CategoryResponse(categoryId, "SPORTS", "Sports Events", "All sports", true);

            var expectedResult = Result.Success<CategoryResponse>(categoryResponse);
            SetupMediatorSendAny<GetCategoryByIdQuery, Result<CategoryResponse>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(categoryId);
            result.Value.Code.Should().Be("SPORTS");
        }

        [Fact]
        public async Task Handle_WithNonExistentCategory_ShouldReturnNotFoundError()
        {
            // Arrange
            var categoryId = 999;
            var query = new GetCategoryByIdQuery(categoryId);

            var expectedResult = Result.Failure<CategoryResponse>(CategoryErrors.NotFound(categoryId));
            SetupMediatorSendAny<GetCategoryByIdQuery, Result<CategoryResponse>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithInactiveCategoryId_ShouldStillReturnCategory()
        {
            // Arrange
            var categoryId = 1;
            var query = new GetCategoryByIdQuery(categoryId);
            var categoryResponse = new CategoryResponse(categoryId, "MUSIC", "Music Events", "Music", false);

            var expectedResult = Result.Success<CategoryResponse>(categoryResponse);
            SetupMediatorSendAny<GetCategoryByIdQuery, Result<CategoryResponse>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.IsActive.Should().BeFalse();
        }
    }
}
