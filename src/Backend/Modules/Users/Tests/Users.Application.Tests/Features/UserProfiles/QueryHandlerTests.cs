using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Users.Domain.Entities;
using Users.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Users.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Queries;
using Users.Application.Features.Users.Dtos;
using Users.Application.Features.Roles.Queries;
using Users.Application.Features.Roles.Dtos;
using Shared.Application.DTOs;
using System.Linq;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Users.Application.Features.Roles.Queries.GetRoleById;

namespace Users.Application.Tests.Features.UserProfiles.Queries
{
    public class GetCurrentUserQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IDeviceDetectionService> _mockDeviceDetectionService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetCurrentUserQueryHandler _handler;

        public GetCurrentUserQueryHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockDeviceDetectionService = new Mock<IDeviceDetectionService>();
            _mockMapper = new Mock<IMapper>();

            _handler = new GetCurrentUserQueryHandler(
                _mockCurrentUserService.Object,
                _mockDeviceDetectionService.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task Handle_WithAuthenticatedUser_ShouldReturnCurrentUserDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetCurrentUserQuery();

            var currentUser = User.Create(
                email: "john@example.com",
                userName: "johndoe",
                passwordHash: "hashed",
                firstName: "John",
                lastName: "Doe"
            );
            currentUser.Id = userId;
            currentUser.IsActive = true;

            var expectedDto = new CurrentUserDto
            {
                UserId = userId,
                Email = currentUser.Email,
                Name = $"{currentUser.FirstName} {currentUser.LastName}".Trim()
            };

            _mockCurrentUserService
                .Setup(c => c.UserId)
                .Returns(userId);

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);

            _mockMapper
                .Setup(m => m.Map<CurrentUserDto>(It.IsAny<User>()))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Email.Should().Be(currentUser.Email);
            result.Value.Name.Should().Contain("John");

            _mockCurrentUserService.VerifyGet(c => c.UserId, Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentCurrentUser_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetCurrentUserQuery();

            _mockCurrentUserService
                .Setup(c => c.UserId)
                .Returns(userId);

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_WithInactiveUser_ShouldStillReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetCurrentUserQuery();

            var inactiveUser = User.Create(
                email: "inactive@example.com",
                userName: "inactiveuser",
                passwordHash: "hashed",
                firstName: null,
                lastName: null
            );
            inactiveUser.Id = userId;
            inactiveUser.IsActive = false;

            var expectedDto = new CurrentUserDto
            {
                UserId = userId,
                Email = inactiveUser.Email
            };

            _mockCurrentUserService
                .Setup(c => c.UserId)
                .Returns(userId);

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveUser);

            _mockMapper
                .Setup(m => m.Map<CurrentUserDto>(It.IsAny<User>()))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }
    }

    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetUserByIdQueryHandler _handler;

        public GetUserByIdQueryHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();

            _handler = new GetUserByIdQueryHandler(
                _mockUserRepository.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task Handle_WithValidUserId_ShouldReturnUserResponseDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery(userId);

            var user = User.Create(
                email: "john@example.com",
                userName: "johndoe",
                passwordHash: "hashed",
                firstName: "John",
                lastName: "Doe"
            );
            user.Id = userId;
            user.IsActive = true;

            var expectedDto = new UserProfileDto
            {
                UserId = userId,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockMapper
                .Setup(m => m.Map<UserProfileDto>(It.IsAny<User>()))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Email.Should().Be(user.Email);
            result.Value.UserName.Should().Be(user.UserName);
            result.Value.FirstName.Should().Be(user.FirstName);
        }

        [Fact]
        public async Task Handle_WithNonExistentUserId_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery(userId);

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
        {
            // Arrange
            var query = new GetUserByIdQuery(Guid.Empty);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }
    }

    public class GetUsersPageQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetUserPageQueryHandler _handler;

        public GetUsersPageQueryHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();

            _handler = new GetUserPageQueryHandler(
                _mockUserRepository.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task Handle_WithValidQuery_ShouldReturnPagedResult()
        {
            // Arrange
            var query = new GetUsersPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Email = null,
                UserName = null,
                FirstName = null,
                LastName = null,
                BirthdayFrom = null,
                BirthdayTo = null,
                Gender = null,
                PhoneNumber = null,
                Status = null
            };

            // Fix CS1929: Repository returns PagedResult<User> (entity), not DTO
            var users = new List<User>
            {
                User.Create("user1@example.com", "user1", "hashed", null, null),
                User.Create("user2@example.com", "user2", "hashed", null, null)
            };
            users[0].Id = Guid.NewGuid();
            users[1].Id = Guid.NewGuid();

            var pagedUsers = PagedResult<User>.Create(
                users,
                pageNumber: 1,
                pageSize: 10,
                totalCount: 2
            );

            _mockUserRepository
                .Setup(r => r.GetAllWithPagingAsync(
                    It.IsAny<PagedQuery>(),
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<IEnumerable<Expression<Func<User, object>>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedUsers);

            // Mapper converts User entities to DTOs
            _mockMapper
                .Setup(m => m.Map<UserProfileDto>(It.IsAny<User>()))
                .Returns((User u) => new UserProfileDto 
                { 
                    UserId = u.Id, 
                    Email = u.Email, 
                    UserName = u.UserName 
                });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(2);
            result.Value.TotalCount.Should().Be(2);
            result.Value.PageNumber.Should().Be(1);
            result.Value.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task Handle_WithEmailFilter_ShouldReturnFilteredUsers()
        {
            // Arrange
            var query = new GetUsersPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Email = "john@example.com",
                UserName = null,
                FirstName = null,
                LastName = null,
                BirthdayFrom = null,
                BirthdayTo = null,
                Gender = null,
                PhoneNumber = null,
                Status = null
            };

            var users = new List<User>
            {
                User.Create("john@example.com", "johndoe", "hashed", null, null)
            };
            users[0].Id = Guid.NewGuid();

            var pagedUsers = PagedResult<User>.Create(
                users,
                pageNumber: 1,
                pageSize: 10,
                totalCount: 1
            );

            _mockUserRepository
                .Setup(r => r.GetAllWithPagingAsync(
                    It.IsAny<PagedQuery>(),
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<IEnumerable<Expression<Func<User, object>>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedUsers);

            _mockMapper
                .Setup(m => m.Map<UserProfileDto>(It.IsAny<User>()))
                .Returns((User u) => new UserProfileDto { UserId = u.Id, Email = u.Email });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(1);
            result.Value.Items[0].Email.Should().Be("john@example.com");
        }

        [Fact]
        public async Task Handle_WithBirthdayDateRange_ShouldReturnFilteredUsers()
        {
            // Arrange
            var query = new GetUsersPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Email = null,
                UserName = null,
                FirstName = null,
                LastName = null,
                BirthdayFrom = new DateTime(1990, 1, 1),
                BirthdayTo = new DateTime(2000, 12, 31),
                Gender = null,
                PhoneNumber = null,
                Status = null
            };

            var users = new List<User>
            {
                User.Create("user@example.com", "user", "hashed", null, null)
            };
            users[0].Id = Guid.NewGuid();

            var pagedUsers = PagedResult<User>.Create(
                users,
                pageNumber: 1,
                pageSize: 10,
                totalCount: 1
            );

            _mockUserRepository
                .Setup(r => r.GetAllWithPagingAsync(
                    It.IsAny<PagedQuery>(),
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<IEnumerable<Expression<Func<User, object>>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedUsers);

            _mockMapper
                .Setup(m => m.Map<UserProfileDto>(It.IsAny<User>()))
                .Returns((User u) => new UserProfileDto { UserId = u.Id });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithInvalidPageNumber_ShouldReturnFailure()
        {
            // Arrange
            var query = new GetUsersPageQuery
            {
                PageNumber = 0,
                PageSize = 10,
                Email = null,
                UserName = null,
                FirstName = null,
                LastName = null,
                BirthdayFrom = null,
                BirthdayTo = null,
                Gender = null,
                PhoneNumber = null,
                Status = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithPageSizeExceedingMaximum_ShouldReturnFailure()
        {
            // Arrange
            var query = new GetUsersPageQuery
            {
                PageNumber = 1,
                PageSize = 1001,
                Email = null,
                UserName = null,
                FirstName = null,
                LastName = null,
                BirthdayFrom = null,
                BirthdayTo = null,
                Gender = null,
                PhoneNumber = null,
                Status = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithNoResults_ShouldReturnEmptyPagedResult()
        {
            // Arrange
            var query = new GetUsersPageQuery
            {
                PageNumber = 2,
                PageSize = 10,
                Email = "nonexistent@example.com",
                UserName = null,
                FirstName = null,
                LastName = null,
                BirthdayFrom = null,
                BirthdayTo = null,
                Gender = null,
                PhoneNumber = null,
                Status = null
            };

            var pagedUsers = PagedResult<User>.Create(
                new List<User>(),
                pageNumber: 2,
                pageSize: 10,
                totalCount: 0
            );

            _mockUserRepository
                .Setup(r => r.GetAllWithPagingAsync(
                    It.IsAny<PagedQuery>(),
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<IEnumerable<Expression<Func<User, object>>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedUsers);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
        }
    }

    public class GetAllRolesQueryHandlerTests
    {
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetAllRolesQueryHandler _handler;

        public GetAllRolesQueryHandlerTests()
        {
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockMapper = new Mock<IMapper>();

            _handler = new GetAllRolesQueryHandler(
                _mockRoleRepository.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnAllRoles()
        {
            // Arrange
            var query = new GetAllRolesQuery();

            var roles = new List<Role>
            {
                Role.Create("Admin", null),
                Role.Create("User", null),
                Role.Create("Moderator", null)
            };
            for (int i = 0; i < roles.Count; i++)
                roles[i].Id = Guid.NewGuid();

            var expectedDtos = new List<RoleResponseDto>
            {
                new RoleResponseDto { Id = roles[0].Id, Name = roles[0].Name },
                new RoleResponseDto { Id = roles[1].Id, Name = roles[1].Name },
                new RoleResponseDto { Id = roles[2].Id, Name = roles[2].Name }
            };

            _mockRoleRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(roles);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<RoleResponseDto>>(It.IsAny<IEnumerable<Role>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(3);
            result.Value.Should().Contain(r => r.Name == "Admin");
            result.Value.Should().Contain(r => r.Name == "User");
            result.Value.Should().Contain(r => r.Name == "Moderator");
        }

        [Fact]
        public async Task Handle_WithNoRoles_ShouldReturnEmptyList()
        {
            // Arrange
            var query = new GetAllRolesQuery();

            _mockRoleRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Role>());

            _mockMapper
                .Setup(m => m.Map<IEnumerable<RoleResponseDto>>(It.IsAny<IEnumerable<Role>>()))
                .Returns(new List<RoleResponseDto>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }

    public class GetRoleByIdQueryHandlerTests
    {
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetRoleByIdQueryHandler _handler;

        public GetRoleByIdQueryHandlerTests()
        {
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockMapper = new Mock<IMapper>();

            _handler = new GetRoleByIdQueryHandler(
                _mockRoleRepository.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task Handle_WithValidRoleId_ShouldReturnRoleResponseDto()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var query = new GetRoleByIdQuery(roleId);

            var role = Role.Create(
                name: "Administrator",
                description: "Admin role"
            );
            role.Id = roleId;

            var expectedDto = new RoleResponseDto
            {
                Id = roleId,
                Name = role.Name,
                Description = role.Description
            };

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _mockMapper
                .Setup(m => m.Map<RoleResponseDto>(It.IsAny<Role>()))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be("Administrator");
            result.Value.Description.Should().Be("Admin role");
        }

        [Fact]
        public async Task Handle_WithNonExistentRoleId_ShouldReturnFailure()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var query = new GetRoleByIdQuery(roleId);

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }
    }
}