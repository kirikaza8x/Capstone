using Xunit;
using Moq;
using FluentAssertions;
using Users.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Users.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Users.Domain.UOW;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Commands.Handlers;
using Shared.Application.DTOs;

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

            SetupUniversalMocks();

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

        private void SetupUniversalMocks()
        {
            // ✅ ICurrentUserService
            _mockCurrentUserService
                .Setup(x => x.UserId)
                .Returns(Guid.Empty);
            _mockCurrentUserService
                .Setup(x => x.Email)
                .Returns((string?)null);
            _mockCurrentUserService
                .Setup(x => x.Name)
                .Returns((string?)null);
            _mockCurrentUserService
                .Setup(x => x.Roles)
                .Returns(Enumerable.Empty<string>());
            _mockCurrentUserService
                .Setup(x => x.Jti)
                .Returns((string?)null);
            _mockCurrentUserService
                .Setup(x => x.IpAddress)
                .Returns((string?)null);
            _mockCurrentUserService
                .Setup(x => x.UserAgent)
                .Returns((string?)null);
            _mockCurrentUserService
                .Setup(x => x.DeviceId)
                .Returns((string?)null);
            _mockCurrentUserService
                .Setup(x => x.GetCurrentUser())
                .Returns((CurrentUserDto?)null);

            // ✅ IDeviceDetectionService - ALL non-nullable DeviceInfo properties MUST be set
            _mockDeviceDetectionService
                .Setup(x => x.GenerateDeviceId())
                .Returns(Guid.NewGuid().ToString());
            
            _mockDeviceDetectionService
                .Setup(x => x.ResolveDeviceName(It.IsAny<string?>()))
                .Returns((string? ua) => string.IsNullOrWhiteSpace(ua) ? "Unknown Device" : "Detected Device");
            
            _mockDeviceDetectionService
                .Setup(x => x.GetDeviceInfo(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .Returns((string? userAgent, string? ipAddress, string? existingDeviceId) => 
                {
                    var deviceId = !string.IsNullOrWhiteSpace(existingDeviceId) 
                        ? existingDeviceId 
                        : Guid.NewGuid().ToString();
                    
                    return new DeviceInfo
                    {
                        DeviceId = deviceId,
                        DeviceName = !string.IsNullOrWhiteSpace(userAgent) ? "Detected Device" : "Unknown Device",
                        Browser = "Chrome",
                        OperatingSystem = "Windows",
                        DeviceType = "Desktop",
                        BrowserVersion = "120.0",
                        OSVersion = "10",
                        UserAgent = userAgent,
                        IpAddress = ipAddress
                    };
                });

            // ✅ IJwtTokenService
            _mockJwtTokenService
                .Setup(j => j.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<string>>()))
                .Returns("default_access_token");
            
            // 🔥 CRITICAL: ExpiryMinutes property used in BuildResponse
            _mockJwtTokenService
                .SetupGet(j => j.ExpiryMinutes)
                .Returns(15);

            // ✅ IRefreshTokenService - Returns RefreshToken entity, NOT tuple
            _mockRefreshTokenService
                .Setup(r => r.GenerateToken(It.IsAny<Guid>()))
                .Returns((Guid userId) => RefreshToken.Create(
                    token: Guid.NewGuid().ToString("N"),
                    expiryDate: DateTime.UtcNow.AddDays(7),
                    userId: userId,
                    deviceId: null,
                    deviceName: null,
                    ipAddress: null,
                    userAgent: null));

            // Default UnitOfWork
            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        }

        [Fact]
        public async Task Handle_WithValidEmailAndPassword_ShouldReturnLoginResponseDto()
        {
            // Arrange
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
            user.AssignRole(Role.Create("User",""));

            var accessToken = "access_token_jwt";
            var refreshTokenEntity = RefreshToken.Create(
                token: "refresh_token_string",
                expiryDate: DateTime.UtcNow.AddDays(7),
                userId: user.Id,
                deviceId: "device_123",
                deviceName: "Test Device",
                ipAddress: "192.168.1.1",
                userAgent: "Mozilla/5.0..."
            );

            _mockUserRepository
                .Setup(r => r.GetUserByMailOrUserNameAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            _mockJwtTokenService
                .Setup(j => j.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<string>>()))
                .Returns(accessToken);

            // ✅ FIXED: Return RefreshToken entity, not tuple
            _mockRefreshTokenService
                .Setup(r => r.GenerateToken(user.Id))
                .Returns(refreshTokenEntity);

            _mockUserRepository
                .Setup(r => r.AddOrUpdateRefreshTokenAsync(
                    It.IsAny<User>(),
                    It.IsAny<RefreshToken>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshTokenEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.AccessToken.Should().Be(accessToken);
            result.Value.RefreshToken.Should().Be(refreshTokenEntity.Token);
            result.Value.DeviceId.Should().NotBeNullOrEmpty();
            result.Value.DeviceName.Should().NotBeNullOrEmpty();

            _mockPasswordHasher.Verify(
                p => p.VerifyPassword(command.Password, user.PasswordHash),
                Times.Once);
            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithUsername_ShouldReturnLoginResponseDto()
        {
            // Arrange
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
            user.AssignRole(Role.Create("User",""));

            var refreshTokenEntity = RefreshToken.Create(
                token: "refresh_token_string",
                expiryDate: DateTime.UtcNow.AddDays(7),
                userId: user.Id,
                deviceId: Guid.NewGuid().ToString(),
                deviceName: "Unknown Device",
                ipAddress: null,
                userAgent: null
            );

            _mockUserRepository
                .Setup(r => r.GetUserByMailOrUserNameAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            // ✅ FIXED: Return RefreshToken entity
            _mockRefreshTokenService
                .Setup(r => r.GenerateToken(user.Id))
                .Returns(refreshTokenEntity);

            _mockUserRepository
                .Setup(r => r.AddOrUpdateRefreshTokenAsync(
                    It.IsAny<User>(),
                    It.IsAny<RefreshToken>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshTokenEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.AccessToken.Should().NotBeNullOrEmpty();
            result.Value.RefreshToken.Should().Be("refresh_token_string");
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
                .Setup(r => r.GetUserByMailOrUserNameAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.Unauthorized);
            result.Error.Description.Should().ContainEquivalentOf("password");

            _mockJwtTokenService.Verify(
                j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()),
                Times.Never);
            _mockRefreshTokenService.Verify(
                r => r.GenerateToken(It.IsAny<Guid>()),
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
                .Setup(r => r.GetUserByMailOrUserNameAsync(
                    It.IsAny<string>(),
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
                .Setup(r => r.GetUserByMailOrUserNameAsync(
                    It.IsAny<string>(),
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
        [InlineData(null)]
        public async Task Handle_WithInvalidEmailOrUserName_ShouldReturnFailure(string? invalidInput)
        {
            // Arrange
            var command = new LoginUserCommand(
                invalidInput!,
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
            user.AssignRole(Role.Create("User",""));

            var refreshTokenEntity = RefreshToken.Create(
                token: "refresh_token_string",
                expiryDate: DateTime.UtcNow.AddDays(7),
                userId: user.Id,
                deviceId: Guid.NewGuid().ToString(),
                deviceName: "Test Device",
                ipAddress: null,
                userAgent: null
            );

            _mockUserRepository
                .Setup(r => r.GetUserByMailOrUserNameAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);

            // ✅ FIXED: Return RefreshToken entity
            _mockRefreshTokenService
                .Setup(r => r.GenerateToken(user.Id))
                .Returns(refreshTokenEntity);

            _mockUserRepository
                .Setup(r => r.AddOrUpdateRefreshTokenAsync(
                    It.IsAny<User>(),
                    It.IsAny<RefreshToken>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshTokenEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            
            _mockRefreshTokenService.Verify(
                r => r.GenerateToken(It.IsAny<Guid>()), 
                Times.Once);
            _mockUserRepository.Verify(
                r => r.AddOrUpdateRefreshTokenAsync(
                    It.IsAny<User>(),
                    It.IsAny<RefreshToken>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}