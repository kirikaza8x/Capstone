using Shared.Domain.Abstractions;

namespace Users.Domain.Errors.Users
{
    public static class UserErrors
    {
        public static Error NotFound => Error.NotFound(
            "User.NotFound", 
            "The user with the specified identifier was not found.");

        public static Error InvalidCredentials => Error.Validation(
            "User.InvalidCredentials", 
            "The email or password provided is incorrect.");

        public static Error Inactive => Error.Validation(
            "User.Inactive", 
            "This account is currently inactive. Please contact support.");

        public static Error EmailAlreadyInUse => Error.Conflict(
            "User.EmailAlreadyInUse", 
            "The provided email is already registered to another account.");

        public static Error SamePassword => Error.Validation(
            "User.SamePassword", 
            "The new password cannot be the same as your old password.");

        public static Error LockedOut => Error.Validation(
            "User.LockedOut", 
            "Account is temporarily locked due to too many failed attempts.");
    }
}