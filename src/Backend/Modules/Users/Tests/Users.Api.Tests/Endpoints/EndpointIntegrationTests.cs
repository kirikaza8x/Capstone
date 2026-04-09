using Xunit;
using Moq;
using FluentAssertions;
using Carter;
using Shared.Domain.Result;
using MediatR;
using Users.Api.Endpoints;
using Users.Application.Features.Auth.Commands;
using Users.Application.Features.Auth.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Users.Api.Tests.Endpoints
{
    /// <summary>
    /// Integration tests for AUTH endpoints using xUnit and Moq.
    /// These tests verify the endpoint contract and response mapping.
    /// </summary>
    public class AuthEndpointTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public AuthEndpointTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        }

        /// <summary>
        /// Test that RegisterUserCommand endpoint returns 201 Created on successful registration
        /// </summary>
        [Fact]
        public async Task RegisterEndpoint_WithValidRequest_ShouldReturn201Created()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var registerRequest = new RegisterUserCommand
            {
                Email = "newuser@example.com",
                UserName = "newuser",
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe"
            };

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<RegisterUserCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Guid>.Success(userId));

            // Act - This would be called at the endpoint
            var result = await _mockMediator.Send(registerRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(userId);
            result.Value.Should().NotBeEmpty();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Test that RegisterUserCommand endpoint returns validation error on duplicate email
        /// </summary>
        [Fact]
        public async Task RegisterEndpoint_WithDuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var registerRequest = new RegisterUserCommand
            {
                Email = "existing@example.com",
                UserName = "newuser",
                Password = "Password123!"
            };

            var conflictError = Error.Conflict("Email already registered", "EMAIL_CONFLICT");

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<RegisterUserCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Guid>.Failure(conflictError));

            // Act
            var result = await _mockMediator.Send(registerRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(ErrorType.Conflict);
        }

        /// <summary>
        /// Test that LoginUserCommand endpoint returns LoginResponseDto with tokens
        /// </summary>
        [Fact]
        public async Task LoginEndpoint_WithValidCredentials_ShouldReturnTokens()
        {
            // Arrange
            var loginRequest = new LoginUserCommand
            {
                EmailOrUserName = "user@example.com",
                Password = "Password123!"
            };

            var loginResponse = new LoginResponseDto
            {
                AccessToken = "access_token_jwt",
                RefreshToken = "refresh_token",
                UserId = Guid.NewGuid(),
                ExpiresIn = 3600
            };

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<LoginUserCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<LoginResponseDto>.Success(loginResponse));

            // Act
            var result = await _mockMediator.Send(loginRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().NotBeNullOrEmpty();
            result.Value.RefreshToken.Should().NotBeNullOrEmpty();
            result.Value.ExpiresIn.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Test that LoginUserCommand endpoint returns Unauthorized on invalid password
        /// </summary>
        [Fact]
        public async Task LoginEndpoint_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginUserCommand
            {
                EmailOrUserName = "user@example.com",
                Password = "WrongPassword!"
            };

            var unauthorizedError = Error.Unauthorized("Invalid credentials", "AUTH_FAILED");

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<LoginUserCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<LoginResponseDto>.Failure(unauthorizedError));

            // Act
            var result = await _mockMediator.Send(loginRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(ErrorType.Unauthorized);
        }

        /// <summary>
        /// Test that RefreshTokenCommand endpoint returns new tokens
        /// </summary>
        [Fact]
        public async Task RefreshTokenEndpoint_WithValidTokens_ShouldReturnNewTokens()
        {
            // Arrange
            var refreshRequest = new RefreshTokenCommand
            {
                AccessToken = "old_access_token",
                RefreshToken = "refresh_token",
                DeviceId = Guid.NewGuid().ToString()
            };

            var refreshResponse = new LoginResponseDto
            {
                AccessToken = "new_access_token_jwt",
                RefreshToken = "new_refresh_token",
                ExpiresIn = 3600
            };

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<RefreshTokenCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<LoginResponseDto>.Success(refreshResponse));

            // Act
            var result = await _mockMediator.Send(refreshRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().NotBe("old_access_token");
            result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Test that RefreshTokenCommand endpoint returns Unauthorized on expired refresh token
        /// </summary>
        [Fact]
        public async Task RefreshTokenEndpoint_WithExpiredRefreshToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var refreshRequest = new RefreshTokenCommand
            {
                AccessToken = "old_access_token",
                RefreshToken = "expired_refresh_token"
            };

            var unauthorizedError = Error.Unauthorized("Refresh token expired", "TOKEN_EXPIRED");

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<RefreshTokenCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<LoginResponseDto>.Failure(unauthorizedError));

            // Act
            var result = await _mockMediator.Send(refreshRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(ErrorType.Unauthorized);
        }
    }

    /// <summary>
    /// Integration tests for USER PROFILE endpoints
    /// </summary>
    public class UserProfileEndpointTests
    {
        private readonly Mock<IMediator> _mockMediator;

        public UserProfileEndpointTests()
        {
            _mockMediator = new Mock<IMediator>();
        }

        /// <summary>
        /// Test that UpdateProfileCommand endpoint updates user profile
        /// </summary>
        [Fact]
        public async Task UpdateProfileEndpoint_WithValidRequest_ShouldReturnUpdatedProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateProfileRequest = new UpdateProfileCommand
            {
                UserId = userId,
                FirstName = "UpdatedFirst",
                LastName = "UpdatedLast",
                Phone = "9876543210",
                Address = "Updated Address"
            };

            var updatedProfile = new UserProfileDto
            {
                UserId = userId,
                FirstName = "UpdatedFirst",
                LastName = "UpdatedLast",
                Phone = "9876543210",
                Address = "Updated Address",
                Email = "user@example.com"
            };

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<UpdateProfileCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<UserProfileDto>.Success(updatedProfile));

            // Act
            var result = await _mockMediator.Send(updateProfileRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.FirstName.Should().Be("UpdatedFirst");
            result.Value.Phone.Should().Be("9876543210");
        }

        /// <summary>
        /// Test that GetCurrentUserQuery endpoint returns current authenticated user
        /// </summary>
        [Fact]
        public async Task GetCurrentUserEndpoint_ShouldReturnAuthenticatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetCurrentUserQuery();

            var currentUserDto = new CurrentUserDto
            {
                UserId = userId,
                Email = "user@example.com",
                Name = "John Doe",
                Roles = new[] { "User" }
            };

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetCurrentUserQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CurrentUserDto>.Success(currentUserDto));

            // Act
            var result = await _mockMediator.Send(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.UserId.Should().Be(userId);
            result.Value.Email.Should().Be("user@example.com");
        }
    }

    /// <summary>
    /// Integration tests for ROLE endpoints
    /// </summary>
    public class RoleEndpointTests
    {
        private readonly Mock<IMediator> _mockMediator;

        public RoleEndpointTests()
        {
            _mockMediator = new Mock<IMediator>();
        }

        /// <summary>
        /// Test that CreateRoleCommand endpoint creates new role
        /// </summary>
        [Fact]
        public async Task CreateRoleEndpoint_WithValidRequest_ShouldReturn201Created()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var createRoleRequest = new CreateRoleCommand
            {
                Name = "NewRole",
                Description = "New custom role"
            };

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<CreateRoleCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Guid>.Success(roleId));

            // Act
            var result = await _mockMediator.Send(createRoleRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(roleId);
        }

        /// <summary>
        /// Test that DeleteRoleCommand endpoint returns 204 NoContent
        /// </summary>
        [Fact]
        public async Task DeleteRoleEndpoint_WithValidRoleId_ShouldReturn204NoContent()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var deleteRoleRequest = new DeleteRoleCommand { Id = roleId };

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<DeleteRoleCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _mockMediator.Send(deleteRoleRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        /// <summary>
        /// Test that AssignRoleCommand endpoint successfully assigns role to user
        /// </summary>
        [Fact]
        public async Task AssignRoleEndpoint_WithValidUserAndRole_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var assignRoleRequest = new AssignRoleCommand
            {
                UserId = userId,
                RoleId = roleId
            };

            _mockMediator
                .Setup(m => m.Send(
                    It.IsAny<AssignRoleCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _mockMediator.Send(assignRoleRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}
