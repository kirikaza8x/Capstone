using Shared.Domain.Abstractions;

namespace Users.Domain.Errors.Otp
{
    public static class OtpErrors
    {
        // Your existing configuration errors...
        public static Error UserNotFound => Error.NotFound(
            "Otp.UserNotFound",
            "No user found with the provided email address.");
        // --- Business Logic Errors ---

        public static Error InvalidCode => Error.Validation(
            "Otp.InvalidCode",
            "The provided OTP code is incorrect.");

        public static Error Expired => Error.Validation(
            "Otp.Expired",
            "The OTP code has expired.");

        public static Error AlreadyUsed => Error.Validation(
            "Otp.AlreadyUsed",
            "This OTP code has already been used.");

        public static Error NotFound => Error.NotFound(
            "Otp.NotFound",
            "No active OTP request found for this user.");
    }
}
