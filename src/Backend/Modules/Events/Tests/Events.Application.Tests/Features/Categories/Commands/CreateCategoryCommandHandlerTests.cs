using Xunit;
using Moq;
using FluentAssertions;
using Events.Application.Categories.Commands.CreateCategory;
using MediatR;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Application.Tests.Features.Categories.Commands
{
    public class CreateCategoryCommandHandlerTests : BaseHandlerTest
    {
        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateCategorySuccessfully()
        {
            // Arrange
            var command = new CreateCategoryCommand(
                "SPORTS",
                "Sports & Games",
                "All sports and gaming events"
            );

            var expectedResult = Result.Success(1);
            SetupMediatorSendAny<CreateCategoryCommand, Result<int>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeGreaterThan(0);

            MockMediator.Verify(
                m => m.Send(
                    It.Is<CreateCategoryCommand>(c =>
                        c.Code == command.Code &&
                        c.Name == command.Name &&
                        c.Description == command.Description),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithDuplicateCode_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateCategoryCommand(
                "MUSIC",
                "Music Events",
                "Live music and concerts"
            );

            var expectedError = CategoryErrors.CodeAlreadyExists(command.Code);
            var expectedResult = Result.Failure<int>(expectedError);
            SetupMediatorSendAny<CreateCategoryCommand, Result<int>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(expectedError);

            MockMediator.Verify(
                m => m.Send(
                    It.IsAny<CreateCategoryCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNullDescription_ShouldCreateCategorySuccessfully()
        {
            // Arrange
            var command = new CreateCategoryCommand(
                "CONFERENCE",
                "Conferences",
                null
            );

            var expectedResult = Result.Success(1);
            SetupMediatorSendAny<CreateCategoryCommand, Result<int>>(expectedResult);

            // Act
            var result = await MockMediator.Object.Send(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeGreaterThan(0);

            MockMediator.Verify(
                m => m.Send(
                    It.IsAny<CreateCategoryCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
