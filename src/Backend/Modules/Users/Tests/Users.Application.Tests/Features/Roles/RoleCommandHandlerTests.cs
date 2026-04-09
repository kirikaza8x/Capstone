using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Users.Application.Features.Roles.Commands;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Entities;
using Users.Domain.Enums;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Users.Domain.Repositories;
using Roles.Domain.UOW;
using Shared.Domain.Abstractions;
using Users.Application.Features.Roles.Commands.CreateRole;

namespace Users.Application.Tests.Features.Roles.Commands
{
    public class CreateRoleCommandHandlerTests
    {
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IRoleUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CreateRoleCommandHandler _handler;

        public CreateRoleCommandHandlerTests()
        {
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockUnitOfWork = new Mock<IRoleUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _handler = new CreateRoleCommandHandler(
                _mockRoleRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateRoleSuccessfully()
        {
            // Arrange
            var command = new CreateRoleCommand(
                "Moderator",
                "Moderates user-generated content"
            );

            var roleId = Guid.NewGuid();
            var createdRole = Role.Create(command.Name, command.Description);
            createdRole.Id = roleId;

            _mockRoleRepository
                .Setup(r => r.AnyAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRoleRepository
                .Setup(r => r.Add(It.IsAny<Role>()))
                .Callback<Role>(r => r.Id = roleId);

            _mockMapper
                .Setup(m => m.Map<RoleResponseDto>(It.IsAny<Role>()))
                .Returns(new RoleResponseDto { Id = roleId, Name = command.Name, Description = command.Description });

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            _mockRoleRepository.Verify(
                r => r.Add(It.Is<Role>(role =>
                    role.Name == command.Name &&
                    role.Description == command.Description)),
                Times.Once);

            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithDuplicateRoleName_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateRoleCommand(
                "Admin",
                "Administrator role"
            );

            _mockRoleRepository
                .Setup(r => r.AnyAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Description.Should().ContainEquivalentOf("already exists");

            _mockRoleRepository.Verify(
                r => r.Add(It.IsAny<Role>()),
                Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_WithInvalidRoleName_ShouldReturnFailure(string invalidName)
        {
            // Arrange
            var command = new CreateRoleCommand(
                invalidName,
                "Test role"
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithMaxLengthRoleName_ShouldCreateSuccessfully()
        {
            // Arrange
            var maxLengthName = new string('A', 100);
            var command = new CreateRoleCommand(
                maxLengthName,
                "Test role"
            );

            _mockRoleRepository
                .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRoleRepository
                .Setup(r => r.Add(It.IsAny<Role>()));

            _mockMapper
                .Setup(m => m.Map<RoleResponseDto>(It.IsAny<Role>()))
                .Returns(new RoleResponseDto { Name = command.Name });

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithExceededMaxLengthRoleName_ShouldReturnFailure()
        {
            // Arrange
            var tooLongName = new string('A', 101);
            var command = new CreateRoleCommand(
                tooLongName,
                "Test role"
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithoutDescription_ShouldCreateSuccessfully()
        {
            // Arrange
            var command = new CreateRoleCommand(
                "Guest",
                null
            );

            _mockRoleRepository
                .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRoleRepository
                .Setup(r => r.Add(It.IsAny<Role>()));

            _mockMapper
                .Setup(m => m.Map<RoleResponseDto>(It.IsAny<Role>()))
                .Returns(new RoleResponseDto { Name = command.Name });

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }

    public class UpdateRoleCommandHandlerTests
    {
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IRoleUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UpdateRoleCommandHandler _handler;

        public UpdateRoleCommandHandlerTests()
        {
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockUnitOfWork = new Mock<IRoleUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _handler = new UpdateRoleCommandHandler(
                _mockRoleRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldUpdateRoleSuccessfully()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var command = new UpdateRoleCommand(
                roleId,
                "UpdatedModerator",
                "Updated description"
            );

            var existingRole = Role.Create(
                "Moderator",
                "Old description"
            );
            existingRole.Id = roleId;

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRole);

            _mockRoleRepository
                .Setup(r => r.AnyAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRoleRepository
                .Setup(r => r.Update(It.IsAny<Role>()));

            _mockMapper
                .Setup(m => m.Map<RoleResponseDto>(It.IsAny<Role>()))
                .Returns(new RoleResponseDto { Id = roleId, Name = command.Name, Description = command.Description });

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(command.Name);

            _mockRoleRepository.Verify(
                r => r.Update(It.Is<Role>(role =>
                    role.Name == command.Name &&
                    role.Description == command.Description)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentRole_ShouldReturnFailure()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var command = new UpdateRoleCommand(
                roleId,
                "UpdatedRole",
                "Updated description"
            );

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.NotFound);

            _mockRoleRepository.Verify(
                r => r.Update(It.IsAny<Role>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithDuplicateRoleName_ShouldReturnFailure()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var command = new UpdateRoleCommand(
                roleId,
                "ExistingRole",
                "Updated description"
            );

            var existingRole = Role.Create(
                "OldName",
                "Old description"
            );
            existingRole.Id = roleId;

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRole);

            _mockRoleRepository
                .Setup(r => r.AnyAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Description.Should().ContainEquivalentOf("already exists");
        }
    }

    public class DeleteRoleCommandHandlerTests
    {
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IRoleUnitOfWork> _mockUnitOfWork;
        private readonly DeleteRoleCommandHandler _handler;

        public DeleteRoleCommandHandlerTests()
        {
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockUnitOfWork = new Mock<IRoleUnitOfWork>();

            _handler = new DeleteRoleCommandHandler(
                _mockRoleRepository.Object,
                _mockUnitOfWork.Object
            );
        }

        [Fact]
        public async Task Handle_WithValidRoleId_ShouldDeleteRoleSuccessfully()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var command = new DeleteRoleCommand(roleId);

            var roleToDelete = Role.Create(
                "ToDelete",
                "This role will be deleted"
            );
            roleToDelete.Id = roleId;

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roleToDelete);

            // Check if role has any assignments - predicate uses Role entity, not RoleAssignment
            _mockRoleRepository
                .Setup(r => r.AnyAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRoleRepository
                .Setup(r => r.Remove(It.IsAny<Role>()));

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockRoleRepository.Verify(
                r => r.Remove(roleToDelete),
                Times.Once);

            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentRole_ShouldReturnFailure()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var command = new DeleteRoleCommand(roleId);

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.NotFound);

            _mockRoleRepository.Verify(
                r => r.Remove(It.IsAny<Role>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithAssignedUsers_ShouldReturnFailure()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var command = new DeleteRoleCommand(roleId);

            var roleWithUsers = Role.Create(
                "AssignedRole",
                "This role is assigned to users"
            );
            roleWithUsers.Id = roleId;

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roleWithUsers);

            // Check if role has assignments - predicate uses Role entity
            _mockRoleRepository
                .Setup(r => r.AnyAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Description.Should().ContainEquivalentOf("assigned");

            _mockRoleRepository.Verify(
                r => r.Remove(It.IsAny<Role>()),
                Times.Never);
        }
    }

    public class AssignRoleCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IRoleUnitOfWork> _mockUnitOfWork;
        private readonly AssignRoleCommandHandler _handler;

        public AssignRoleCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockUnitOfWork = new Mock<IRoleUnitOfWork>();

            _handler = new AssignRoleCommandHandler(
                _mockUserRepository.Object,
                _mockRoleRepository.Object,
                _mockUnitOfWork.Object
            );
        }

        [Fact]
        public async Task Handle_WithValidUserAndRole_ShouldAssignSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var command = new AssignRoleCommand(userId, roleId);

            var user = User.Create(
                email: "user@example.com",
                userName: "testuser",
                passwordHash: "hashed",
                firstName: null,
                lastName: null
            );
            user.Id = userId;

            var role = Role.Create(
                name: "Admin",
                description: null
            );
            role.Id = roleId;

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            // Check if assignment already exists - predicate uses User entity for repository consistency
            _mockUserRepository
                .Setup(r => r.AnyAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var command = new AssignRoleCommand(userId, roleId);

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_WithNonExistentRole_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var command = new AssignRoleCommand(userId, roleId);

            var user = User.Create(
                email: "user@example.com",
                userName: "testuser",
                passwordHash: "hashed",
                firstName: null,
                lastName: null
            );
            user.Id = userId;

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_WithAlreadyAssignedRole_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var command = new AssignRoleCommand(userId, roleId);

            var user = User.Create(
                email: "user@example.com",
                userName: "testuser",
                passwordHash: "hashed",
                firstName: null,
                lastName: null
            );
            user.Id = userId;

            var role = Role.Create(
                name: "Admin",
                description: null
            );
            role.Id = roleId;

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockRoleRepository
                .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            // Check if assignment already exists - predicate uses User entity
            _mockUserRepository
                .Setup(r => r.AnyAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Description.Should().ContainEquivalentOf("already");
        }
    }
}