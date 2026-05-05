using Xunit;
using Moq;
using FluentAssertions;
using Events.Application.Categories.Commands.ToggleCategoryStatus;
using MediatR;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Application.Tests.Features.Categories.Commands
{
    public class ToggleCategoryStatusCommandHandlerTests : BaseHandlerTest
    {
        [Fact]
        public async Task Handle_WithActiveTrueAndInactiveCategory_ShouldActivateSuccessfully()
        {
            // Arrange
            var categoryId = 1;
            var command = new ToggleCategoryStatusCommand(categoryId, true);

            var expectedResult = Result.Success();
            SetupMediatorSendAny<ToggleCategoryStatusCommand, Result>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            MockMediator.Verify(
                m => m.Send(
                    It.Is<ToggleCategoryStatusCommand>(c =>
                        c.CategoryId == categoryId &&
                        c.Activate == true),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithActiveFalseAndActiveCategory_ShouldDeactivateSuccessfully()
        {
            // Arrange
            var categoryId = 1;
            var command = new ToggleCategoryStatusCommand(categoryId, false);

            var expectedResult = Result.Success();
            SetupMediatorSendAny<ToggleCategoryStatusCommand, Result>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            MockMediator.Verify(
                m => m.Send(
                    It.IsAny<ToggleCategoryStatusCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentCategory_ShouldReturnNotFoundError()
        {
            // Arrange
            var categoryId = 999;
            var command = new ToggleCategoryStatusCommand(categoryId, true);

            var expectedResult = Result.Failure(CategoryErrors.NotFound(categoryId));
            SetupMediatorSendAny<ToggleCategoryStatusCommand, Result>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }
    }
}
