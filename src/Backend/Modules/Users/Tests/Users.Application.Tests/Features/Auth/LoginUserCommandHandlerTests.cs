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
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Commands.Handlers;

namespace Users.Application.Tests.Features.Auth
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IJwtTokenService> _mockJwtTokenService;
        private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IDeviceDetectionService> _mockDeviceDetectionService;
        private readonly Mock<IUserUnitOfWork> _mockUnitOfWork;
        private readonly LoginUserCommandHandler _handler;

        public LoginUserCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockJwtTokenService = new Mock<IJwtTokenService>();
            _mockRefreshTokenService = new Mock<IRefreshTokenService>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockDeviceDetectionService = new Mock<IDeviceDetectionService>();
            _mockUnitOfWork = new Mock<IUserUnitOfWork>();

            _handler = new LoginUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockJwtTokenService.Object,
                _mockRefreshTokenService.Object,
                _mockCurrentUserService.Object,
                _mockDeviceDetectionService.Object,
                _mockUnitOfWork.Object
            );
        }

        [Fact]
        public async Task Handle_WithValidEmailAndPassword_ShouldReturnLoginResponseDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new LoginUserCommand(
                "test@example.com",
                "Password123!",
                "Chrome",
                "192.168.1.1",
                "Mozilla/5.0..."
            );

            var user = User.Create(
                "test@example.com",
                "testuser",
                "hashed_password",
                "John",
                "Doe"
            );

            var accessToken = "access_token_jwt";
            var refreshTokenEntity = RefreshToken.Create(
                token: "refresh_token_string",
                expiryDate: DateTime.UtcNow.AddDays(7),
                userId: user.Id
            );

            _mockUserRepository
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            _mockJwtTokenService
                .Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Collections.Generic.IReadOnlyList<string>>()))
                .Returns(accessToken);

            _mockRefreshTokenService
                .Setup(r => r.GenerateToken(user.Id))
                .Returns(refreshTokenEntity);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.AccessToken.Should().Be(accessToken);
            result.Value.RefreshToken.Should().Be(refreshTokenEntity.Token);

            _mockPasswordHasher.Verify(
                p => p.VerifyPassword(command.Password, user.PasswordHash),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithUsername_ShouldReturnLoginResponseDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new LoginUserCommand(
                "testuser",
                "Password123!",
                null,
                null,
                null
            );

            var user = User.Create(
                "test@example.com",
                "testuser",
                "hashed_password"
            );

            var refreshTokenEntity = RefreshToken.Create(
                token: "refresh_token_string",
                expiryDate: DateTime.UtcNow.AddDays(7),
                userId: user.Id
            );

            _mockUserRepository
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            _mockJwtTokenService
                .Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Collections.Generic.IReadOnlyList<string>>()))
                .Returns("access_token");

            _mockRefreshTokenService
                .Setup(r => r.GenerateToken(It.IsAny<Guid>()))
                .Returns(refreshTokenEntity);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WithInvalidPassword_ShouldReturnFailure()
        {
            // Arrange
            var command = new LoginUserCommand(
                "test@example.com",
                "WrongPassword123!",
                null,
                null,
                null
            );

            var user = User.Create(
                "test@example.com",
                "testuser",
                "hashed_password"
            );

            _mockUserRepository
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Description.Should().ContainEquivalentOf("password");

            _mockJwtTokenService.Verify(
                j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Collections.Generic.IReadOnlyList<string>>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
        {
            // Arrange
            var command = new LoginUserCommand(
                "nonexistent@example.com",
                "Password123!",
                null,
                null,
                null
            );

            _mockUserRepository
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.NotFound);

            _mockPasswordHasher.Verify(
                p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithInactiveUser_ShouldReturnFailure()
        {
            // Arrange
            var command = new LoginUserCommand(
                "inactive@example.com",
                "Password123!",
                null,
                null,
                null
            );

            var user = User.Create(
                "inactive@example.com",
                "inactiveuser",
                "hashed_password"
            );
            user.Deactivate();

            _mockUserRepository
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Description.Should().ContainEquivalentOf("inactive");
        }

        [Theory]
        [InlineData("")]
        public async Task Handle_WithInvalidEmailOrUserName_ShouldReturnFailure(string invalidInput)
        {
            // Arrange
            var command = new LoginUserCommand(
                invalidInput,
                "Password123!",
                null,
                null,
                null
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldCreateRefreshTokenRecord()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new LoginUserCommand(
                "test@example.com",
                "Password123!",
                null,
                null,
                null
            );

            var user = User.Create(
                "test@example.com",
                "testuser",
                "hashed_password"
            );

            var refreshTokenEntity = RefreshToken.Create(
                token: "refresh_token_string",
                expiryDate: DateTime.UtcNow.AddDays(7),
                userId: user.Id
            );

            _mockUserRepository
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            _mockJwtTokenService
                .Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Collections.Generic.IReadOnlyList<string>>()))
                .Returns("access_token");

            _mockRefreshTokenService
                .Setup(r => r.GenerateToken(It.IsAny<Guid>()))
                .Returns(refreshTokenEntity);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRefreshTokenService.Verify(r => r.GenerateToken(It.IsAny<Guid>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}