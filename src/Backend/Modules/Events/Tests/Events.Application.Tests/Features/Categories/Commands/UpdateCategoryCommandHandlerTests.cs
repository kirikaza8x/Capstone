using Xunit;
using Moq;
using FluentAssertions;
using Events.Application.Categories.Commands.UpdateCategory;
using MediatR;
using static Events.Domain.Errors.EventErrors;
using Shared.Domain.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Application.Tests.Features.Categories.Commands
{
    public class UpdateCategoryCommandHandlerTests : BaseHandlerTest
    {
        [Fact]
        public async Task Handle_WithValidCommand_ShouldUpdateCategorySuccessfully()
        {
            // Arrange
            var categoryId = 1;
            var command = new UpdateCategoryCommand(
                categoryId,
                "SPORTS_UPDATED",
                "Sports & Games Updated",
                "Updated description"
            );

            var expectedResult = Result.Success();
            SetupMediatorSendAny<UpdateCategoryCommand, Result>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            MockMediator.Verify(
                m => m.Send(
                    It.Is<UpdateCategoryCommand>(c =>
                        c.CategoryId == categoryId &&
                        c.Code == command.Code),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentCategory_ShouldReturnNotFoundError()
        {
            // Arrange
            var categoryId = 999;
            var command = new UpdateCategoryCommand(
                categoryId,
                "UPDATED",
                "Updated Name",
                "Description"
            );

            var expectedResult = Result.Failure(CategoryErrors.NotFound(categoryId));
            SetupMediatorSendAny<UpdateCategoryCommand, Result>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithDuplicateCodeForDifferentCategory_ShouldReturnFailure()
        {
            // Arrange
            var categoryId = 1;
            var command = new UpdateCategoryCommand(
                categoryId,
                "EXISTING_CODE",
                "New Name",
                "Description"
            );

            var expectedResult = Result.Failure(CategoryErrors.CodeAlreadyExists(command.Code));
            SetupMediatorSendAny<UpdateCategoryCommand, Result>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }
    }
}
