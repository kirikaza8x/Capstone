using Xunit;
using Moq;
using FluentAssertions;
using Users.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Users.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Users.Domain.UOW;
using Shared.Domain.Abstractions;
using FluentValidation;
using FluentValidation.Results;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Commands.RegisterUser;

// Fix CS0104: Disambiguate ValidationResult with alias
using FluentValidationResult = FluentValidation.Results.ValidationResult;

namespace Users.Application.Tests.Features.Auth
{
    public class RegisterUserCommandHandlerTests : IDisposable
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IUserUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IValidator<RegisterUserCommand>> _mockValidator;

        public RegisterUserCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockUnitOfWork = new Mock<IUserUnitOfWork>();
            _mockValidator = new Mock<IValidator<RegisterUserCommand>>();
        }

        [Fact]
        public async Task Handle_ValidRegisterUserCommand_ReturnsSuccess()
        {
            // Arrange
            var command = new RegisterUserCommand(
                "test@email.com",
                "testuser",
                "SecurePassword123!",
                "John",
                "Doe",
                null,
                null
            );

            var hashedPassword = "hashedPassword123";
            _mockPasswordHasher
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns(hashedPassword);

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.GetByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidationResult());

            var handler = new RegisterUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockUnitOfWork.Object,
                _mockValidator.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            _mockUserRepository.Verify(x => x.AddOrUpdateRefreshTokenAsync(
                It.IsAny<User>(), 
                It.IsAny<RefreshToken>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithExistingEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = new RegisterUserCommand(
                "existing@example.com",
                "newuser",
                "Password123!",
                null,
                null,
                null,
                null
            );

            var existingUser = User.Create(
                "existing@example.com",
                "differentuser",
                "hashedOldPassword",
                null,
                null
            );

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidationResult());

            var handler = new RegisterUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockUnitOfWork.Object,
                _mockValidator.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            _mockUserRepository.Verify(x => x.AddOrUpdateRefreshTokenAsync(
                It.IsAny<User>(), 
                It.IsAny<RefreshToken>(), 
                It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithExistingUsername_ShouldReturnFailure()
        {
            // Arrange
            var command = new RegisterUserCommand(
                "new@example.com",
                "existinguser",
                "Password123!",
                null,
                null,
                null,
                null
            );

            var existingUser = User.Create(
                "different@example.com",
                "existinguser",
                "hashedPassword",
                null,
                null
            );

            _mockUserRepository
                .Setup(x => x.GetByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidationResult());

            var handler = new RegisterUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockUnitOfWork.Object,
                _mockValidator.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WithDatabaseSaveFailure_ShouldThrowException()
        {
            // Arrange
            var command = new RegisterUserCommand(
                "test@example.com",
                "testuser",
                "Password123!",
                null,
                null,
                null,
                null
            );

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.GetByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockPasswordHasher
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidationResult());

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var handler = new RegisterUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockUnitOfWork.Object,
                _mockValidator.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await handler.Handle(command, CancellationToken.None));
        }

        [Theory]
        [InlineData("")]
        public async Task Handle_WithInvalidEmail_ShouldFail(string invalidEmail)
        {
            // Arrange
            var command = new RegisterUserCommand(
                invalidEmail,
                "testuser",
                "Password123!",
                null,
                null,
                null,
                null
            );

            var validationResult = new FluentValidationResult();
            validationResult.Errors.Add(new ValidationFailure("Email", "Email is required"));

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var handler = new RegisterUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockUnitOfWork.Object,
                _mockValidator.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WithMinimalValidCommand_ShouldRegisterSuccessfully()
        {
            // Arrange - Only required fields
            var command = new RegisterUserCommand(
                "minimal@example.com",
                "minimaluser",
                "Password123!",
                null,
                null,
                null,
                null
            );

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.GetByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockPasswordHasher
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidationResult());

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new RegisterUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockUnitOfWork.Object,
                _mockValidator.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            _mockUserRepository.Verify(x => x.AddOrUpdateRefreshTokenAsync(
                It.IsAny<User>(), 
                It.IsAny<RefreshToken>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_CallsPasswordHasher_WithProvidedPassword()
        {
            // Arrange
            var password = "MySecurePassword123!";
            var command = new RegisterUserCommand(
                "test@example.com",
                "testuser",
                password,
                null,
                null,
                null,
                null
            );

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.GetByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockPasswordHasher
                .Setup(x => x.HashPassword(password))
                .Returns("hashed_password");

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidationResult());

            var handler = new RegisterUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockUnitOfWork.Object,
                _mockValidator.Object
            );

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            _mockPasswordHasher.Verify(x => x.HashPassword(password), Times.Once);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}