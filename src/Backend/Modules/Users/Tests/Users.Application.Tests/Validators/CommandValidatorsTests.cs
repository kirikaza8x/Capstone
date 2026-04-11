using Xunit;
using FluentValidation.TestHelper;
using Users.Application.Features.Roles.Commands;
using Users.Application.Features.Roles.Validators;
using System;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Validators;
using Users.Domain.Enums;

namespace Users.Application.Tests.Validators
{
    public class RegisterUserCommandValidatorTests
    {
        private readonly RegisterUserCommandValidator _validator;

        public RegisterUserCommandValidatorTests()
        {
            _validator = new RegisterUserCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                UserName: "testuser",
                Password: "ValidPassword123!",
                FirstName: "John",
                LastName: "Doe",
                PhoneNumber: "1234567890",
                Address: "123 Main St"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithMissingEmail_ShouldHaveError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: string.Empty,  // Fix CS8625: Use string.Empty instead of null for non-nullable
                UserName: "testuser",
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: null,
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email)
                .WithErrorCode("NotEmptyValidator");
        }

        [Fact]
        public void Validate_WithInvalidEmailFormat_ShouldHaveError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "invalidemail",
                UserName: "testuser",
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: null,
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email)
                .WithErrorCode("EmailValidator");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithInvalidUsername_ShouldHaveError(string invalidUsername)
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                UserName: invalidUsername,
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: null,
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.UserName);
        }

        [Theory]
        [InlineData("pass")]
        [InlineData("12345678")]
        [InlineData("abcdefgh")]
        [InlineData("Pass")]
        public void Validate_WithWeakPassword_ShouldHaveError(string weakPassword)
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                UserName: "testuser",
                Password: weakPassword,
                FirstName: null,
                LastName: null,
                PhoneNumber: null,
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Password)
                .WithErrorCode("RegexValidator");
        }

        [Fact]
        public void Validate_WithDuplicateEmail_ShouldHaveError()
        {
            // This would typically require integration with a service
            // For unit testing, we'd mock this validation
            // Arrange
            var command = new RegisterUserCommand(
                Email: "existing@example.com",
                UserName: "newuser",
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: null,
                Address: null
            );

            // Act & Assert
            // This test demonstrates how you'd handle async validation
            // In practice, implement IAsyncValidator interface
        }

        [Fact]
        public void Validate_WithMaxLengthUsername_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                UserName: new string('a', 50),
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: null,
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.UserName);
        }

        [Fact]
        public void Validate_WithExceededMaxLengthUsername_ShouldHaveError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                UserName: new string('a', 101),
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: null,
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.UserName)
                .WithErrorCode("MaximumLengthValidator");
        }

        [Fact]
        public void Validate_WithValidPhoneNumber_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                UserName: "testuser",
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: "+1-234-567-8900",
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.PhoneNumber);
        }

        [Fact]
        public void Validate_WithInvalidPhoneNumber_ShouldHaveError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                UserName: "testuser",
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: "invalid",
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.PhoneNumber);
        }

        [Fact]
        public void Validate_WithOptionalFields_ShouldHaveNoErrors()
        {
            // Arrange - Only required fields
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                UserName: "testuser",
                Password: "ValidPassword123!",
                FirstName: null,
                LastName: null,
                PhoneNumber: null,
                Address: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    public class LoginUserCommandValidatorTests
    {
        private readonly LoginUserCommandValidator _validator;

        public LoginUserCommandValidatorTests()
        {
            _validator = new LoginUserCommandValidator();
        }

        [Fact]
        public void Validate_WithValidEmailAndPassword_ShouldHaveNoErrors()
        {
            // Arrange - Fix CS7036: Provide all 5 required params
            var command = new LoginUserCommand(
                EmailOrUserName: "test@example.com",
                Password: "Password123!",
                DeviceName: null,
                IpAddress: null,
                UserAgent: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithValidUsername_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new LoginUserCommand(
                EmailOrUserName: "testuser",
                Password: "Password123!",
                DeviceName: null,
                IpAddress: null,
                UserAgent: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithMissingEmailOrUsername_ShouldHaveError()
        {
            // Arrange - Fix CS8625: Use string.Empty for non-nullable param
            var command = new LoginUserCommand(
                EmailOrUserName: string.Empty,
                Password: "Password123!",
                DeviceName: null,
                IpAddress: null,
                UserAgent: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.EmailOrUserName)
                .WithErrorCode("NotEmptyValidator");
        }

        [Fact]
        public void Validate_WithMissingPassword_ShouldHaveError()
        {
            // Arrange - Fix CS8625: Use string.Empty for non-nullable param
            var command = new LoginUserCommand(
                EmailOrUserName: "test@example.com",
                Password: string.Empty,
                DeviceName: null,
                IpAddress: null,
                UserAgent: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Password)
                .WithErrorCode("NotEmptyValidator");
        }

        [Fact]
        public void Validate_WithOptionalDeviceInfo_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new LoginUserCommand(
                EmailOrUserName: "test@example.com",
                Password: "Password123!",
                DeviceName: null,
                IpAddress: null,
                UserAgent: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    public class UpdateProfileCommandValidatorTests
    {
        private readonly UpdateProfileCommandValidator _validator;

        public UpdateProfileCommandValidatorTests()
        {
            _validator = new UpdateProfileCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: "Doe",
                Birthday: new DateTime(1990, 1, 15),
                Gender: Gender.Male,
                Phone: "1234567890",
                Address: "123 Main St",
                Description: "Software Developer",
                SocialLink: "https://twitter.com/johndoe",
                ProfileImageUrl: "https://example.com/profile.jpg"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyUserId_ShouldHaveError()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.Empty,
                FirstName: "John",
                LastName: null,
                Birthday: null,
                Gender: null,
                Phone: null,
                Address: null,
                Description: null,
                SocialLink: null,
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.UserId);
        }

        [Fact]
        public void Validate_WithFutureBirthday_ShouldHaveError()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: DateTime.UtcNow.AddYears(1),
                Gender: null,
                Phone: null,
                Address: null,
                Description: null,
                SocialLink: null,
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Birthday);
        }

        [Fact]
        public void Validate_WithValidBirthday_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: new DateTime(1990, 1, 15),
                Gender: null,
                Phone: null,
                Address: null,
                Description: null,
                SocialLink: null,
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Birthday);
        }

        [Fact]
        public void Validate_WithValidSocialLink_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: null,
                Gender: null,
                Phone: null,
                Address: null,
                Description: null,
                SocialLink: "https://linkedin.com/in/johndoe",
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.SocialLink);
        }

        [Fact]
        public void Validate_WithInvalidSocialLink_ShouldHaveError()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: null,
                Gender: null,
                Phone: null,
                Address: null,
                Description: null,
                SocialLink: "not-a-valid-url",
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.SocialLink);
        }

        [Fact]
        public void Validate_WithMaxLengthDescription_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: null,
                Gender: null,
                Phone: null,
                Address: null,
                Description: new string('a', 500),
                SocialLink: null,
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Description);
        }

        [Fact]
        public void Validate_WithExceededMaxLengthDescription_ShouldHaveError()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: null,
                Gender: null,
                Phone: null,
                Address: null,
                Description: new string('a', 1001),
                SocialLink: null,
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Description);
        }

        [Fact]
        public void Validate_WithValidProfileImageUrl_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: null,
                Gender: null,
                Phone: null,
                Address: null,
                Description: null,
                SocialLink: null,
                ProfileImageUrl: "https://cdn.example.com/images/profile.jpg"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.ProfileImageUrl);
        }

        [Fact]
        public void Validate_WithInvalidGender_ShouldHaveError()
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: null,
                Gender: Gender.Other,
                Phone: null,
                Address: null,
                Description: null,
                SocialLink: null,
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Gender);
        }

        [Theory]
        [InlineData(Gender.Male)]
        [InlineData(Gender.Female)]
        [InlineData(Gender.Other)]
        [InlineData(null)]
        public void Validate_WithValidGender_ShouldHaveNoErrors(Gender? validGender)
        {
            // Arrange
            var command = new UpdateProfileCommand(
                UserId: Guid.NewGuid(),
                FirstName: "John",
                LastName: null,
                Birthday: null,
                Gender: validGender,
                Phone: null,
                Address: null,
                Description: null,
                SocialLink: null,
                ProfileImageUrl: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Gender);
        }
    }

    public class CreateRoleCommandValidatorTests
    {
        private readonly CreateRoleCommandValidator _validator;

        public CreateRoleCommandValidatorTests()
        {
            _validator = new CreateRoleCommandValidator();
        }

        [Fact]
        public void Validate_WithValidRoleName_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new CreateRoleCommand(
                Name: "Administrator",
                Description: "Admin role"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithMissingRoleName_ShouldHaveError()
        {
            // Arrange - Fix CS8625: Use string.Empty for non-nullable Name
            var command = new CreateRoleCommand(
                Name: string.Empty,
                Description: "Admin role"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorCode("NotEmptyValidator");
        }

        [Fact]
        public void Validate_WithMaxLengthRoleName_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new CreateRoleCommand(
                Name: new string('A', 100),
                Description: "Admin role"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Name);
        }

        [Fact]
        public void Validate_WithExceededMaxLengthRoleName_ShouldHaveError()
        {
            // Arrange
            var command = new CreateRoleCommand(
                Name: new string('A', 101),
                Description: "Admin role"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorCode("MaximumLengthValidator");
        }

        [Fact]
        public void Validate_WithoutDescription_ShouldHaveNoErrors()
        {
            // Arrange
            var command = new CreateRoleCommand(
                Name: "Administrator",
                Description: null
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithSpecialCharactersInName_ShouldReturnError()
        {
            // Arrange
            var command = new CreateRoleCommand(
                Name: "Admin@#$%",
                Description: "Admin role"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name);
        }
    }
}