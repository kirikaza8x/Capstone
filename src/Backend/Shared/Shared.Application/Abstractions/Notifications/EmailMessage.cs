namespace Shared.Application.Abstractions.Notifications
{
    public sealed record EmailMessage(
        string To,
        string Subject,
        string Body,
        bool IsHtml = false);
}
