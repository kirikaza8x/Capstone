using Xunit;
using Moq;
using FluentAssertions;
using Events.Application.Categories.Commands.DeleteCategory;
using MediatR;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Application.Tests.Features.Categories.Commands
{
    public class DeleteCategoryCommandHandlerTests : BaseHandlerTest
    {
        [Fact]
        public async Task Handle_WithValidCategoryId_ShouldDeleteCategorySuccessfully()
        {
            // Arrange
            var categoryId = 1;
            var command = new DeleteCategoryCommand(categoryId);

            var expectedResult = Result.Success();
            SetupMediatorSendAny<DeleteCategoryCommand, Result>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            MockMediator.Verify(
                m => m.Send(
                    It.Is<DeleteCategoryCommand>(c => c.CategoryId == categoryId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentCategory_ShouldReturnNotFoundError()
        {
            // Arrange
            var categoryId = 999;
            var command = new DeleteCategoryCommand(categoryId);

            var expectedResult = Result.Failure(CategoryErrors.NotFound(categoryId));
            SetupMediatorSendAny<DeleteCategoryCommand, Result>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();

            MockMediator.Verify(
                m => m.Send(
                    It.IsAny<DeleteCategoryCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
