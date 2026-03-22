using Shared.Domain.Abstractions;

namespace Notifications.Domain.Errors;

public static class NotificationErrors
{
    public static class Email
    {
        public static Error UserNotFound(Guid userId) => Error.NotFound(
            "Notification.Email.UserNotFound",
            $"User with id '{userId}' not found or has no email.");
    }
}
