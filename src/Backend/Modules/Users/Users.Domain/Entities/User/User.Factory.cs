using Users.Domain.Enums;
using Users.Domain.Events;

namespace Users.Domain.Entities;

public partial class User
{

    // --------------------
    // Factory
    // --------------------
    public static User Create(
        string? email,
        string userName,
        string passwordHash,
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null,
        string? address = null,
        string? profileImageUrl = null,
        Role? role = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            PasswordHash = passwordHash,
            ProfileImageUrl = profileImageUrl,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Address = address
        };

        if (role != null)
            user.AssignRole(role);

        user.Status = UserStatus.Active;
        user.RaiseDomainEvent(
            new UserCreatedEvent(user.Id, user.Email ?? string.Empty, user.UserName)
        );

        return user;
    }
    public static User CreateSheet(
        string? email,
        string userName,
        string passwordHash,
        string? firstName,
        string? lastName,
        string? phoneNumber,
        string? address,
        DateTime? birthday,
        Gender? gender,
        string? description,
        string? socialLink,
        string? profileImageUrl = null,
        UserStatus status = UserStatus.Active)
    {
        // if (string.IsNullOrWhiteSpace(userName))
        //     throw new ArgumentException("Username is required.", nameof(userName));

        // if (string.IsNullOrWhiteSpace(passwordHash))
        //     throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));

        var user = new User
        {
            Email = email,
            UserName = userName,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Address = address,
            Birthday = birthday,
            Gender = gender,
            Description = description,
            SocialLink = socialLink,
            ProfileImageUrl = profileImageUrl,
            Status = status,
        };

        return user;
    }
}
