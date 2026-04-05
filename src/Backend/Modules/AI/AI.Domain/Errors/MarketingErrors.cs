using Marketing.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Marketing.Domain.Errors;

public static class MarketingErrors
{
    // =========================================================
    // Post Errors
    // =========================================================
    public static class Post
    {
        public static Error TrackingTokenAlreadyExists(string token) => Error.Validation(
            "Post.TrackingTokenAlreadyExists",
            $"Tracking token '{token}' is already in use.");

        public static Error EventIdRequired => Error.Validation(
            "Post.EventIdRequired",
            "EventId is required.");

        public static Error OrganizerIdRequired => Error.Validation(
            "Post.OrganizerIdRequired",
            "OrganizerId is required.");

        public static Error TitleCannotBeEmpty => Error.Validation(
            "Post.TitleCannotBeEmpty",
            "Title cannot be empty.");

        public static Error BodyCannotBeEmpty => Error.Validation(
            "Post.BodyCannotBeEmpty",
            "Body cannot be empty.");

        public static Error CreateFailed(string reason) => Error.Failure(
            "Post.CreateFailed",
            $"Failed to create post: {reason}");

        public static Error NotFound(Guid postId) => Error.NotFound(
            "Post.NotFound",
            $"Post with ID '{postId}' was not found.");

        public static Error NotAuthorized(Guid organizerId) => Error.Forbidden(
            "Post.NotAuthorized",
            $"Organizer '{organizerId}' is not authorized to modify this post.");

        public static Error CannotEditInStatus(PostStatus status) => Error.Validation(
            "Post.CannotEditInStatus",
            $"Cannot edit post in status '{status}'.");

        public static Error CannotSubmitInStatus(PostStatus status) => Error.Validation(
            "Post.CannotSubmitInStatus",
            $"Cannot submit post in status '{status}'.");

        public static Error CannotPublishInStatus(PostStatus status) => Error.Validation(
            "Post.CannotPublishInStatus",
            $"Cannot publish post in status '{status}'.");

        public static Error CannotApproveInStatus(PostStatus status) => Error.Validation(
            "Post.CannotApproveInStatus",
            $"Cannot approve post in status '{status}'.");

        public static Error CannotRejectInStatus(PostStatus status) => Error.Validation(
            "Post.CannotRejectInStatus",
            $"Cannot reject post in status '{status}'.");

        public static Error ContentIncompleteForSubmit => Error.Validation(
            "Post.ContentIncompleteForSubmit",
            "Title and body are required before submitting.");

        public static Error CannotArchiveWhilePending => Error.Validation(
            "Post.CannotArchiveWhilePending",
            "Cannot archive post while it is pending review.");

        public static Error ReviewerRequired => Error.Validation(
            "Post.ReviewerRequired",
            "Admin reviewer ID is required.");

        public static Error RejectionReasonRequired => Error.Validation(
            "Post.RejectionReasonRequired",
            "Rejection reason is required.");

        public static Error ExternalUrlRequired => Error.Validation(
            "Post.ExternalUrlRequired",
            "External URL is required.");

        public static Error CannotRecordDistributionInStatus(PostStatus status) => Error.Validation(
            "Post.CannotRecordDistributionInStatus",
            $"Cannot record external distribution for post in status '{status}'.");

        public static Error UpdateFailed(string reason) => Error.Failure(
            "Post.UpdateFailed",
            $"Failed to update post: {reason}");

        public static Error SubmitFailed(string reason) => Error.Failure(
            "Post.SubmitFailed",
            $"Failed to submit post: {reason}");

        public static Error ApproveFailed(string reason) => Error.Failure(
            "Post.ApproveFailed",
            $"Failed to approve post: {reason}");

        public static Error RejectFailed(string reason) => Error.Failure(
            "Post.RejectFailed",
            $"Failed to reject post: {reason}");

        public static Error PublishFailed(string reason) => Error.Failure(
            "Post.PublishFailed",
            $"Failed to publish post: {reason}");

        public static Error ForceRemoveFailed(string reason) => Error.Failure(
            "Post.ForceRemoveFailed",
            $"Failed to remove post: {reason}");

        // ── NEW: For external distribution ──
        public static Error CannotDistributeInStatus(PostStatus status) => Error.Validation(
            "Post.CannotDistributeInStatus",
            $"Cannot distribute post to external platform while in status '{status}'. Only Published posts can be distributed.");

        public static Error DistributionAlreadyQueued(ExternalPlatform platform) => Error.Validation(
            "Post.DistributionAlreadyQueued",
            $"Post is already queued for distribution to {platform}.");
    }

    // =========================================================
    // Distribution Errors (NEW - sibling of Post, not nested)
    // =========================================================
    public static class Distribution
    {
        public static Error PlatformRequired => Error.Validation(
            "Distribution.PlatformRequired",
            "Platform must be specified and cannot be Unknown.");

        public static Error UrlRequired => Error.Validation(
            "Distribution.UrlRequired",
            "External URL is required when confirming distribution.");

        public static Error NotFound(ExternalPlatform platform) => Error.NotFound(
            "Distribution.NotFound",
            $"No pending distribution record found for platform '{platform}'.");

        public static Error AlreadyQueued(ExternalPlatform platform) => Error.Conflict(
            "Distribution.AlreadyQueued",
            $"A distribution to {platform} is already pending. Wait for completion or remove it first.");

        public static Error ExternalPostIdMissing => Error.Validation(
            "Distribution.ExternalPostIdMissing",
            "No external post ID stored for this distribution.");

        public static Error MetricsFetchFailed => Error.Failure(
            "Distribution.MetricsFetchFailed",
            "Failed to fetch metrics from Facebook API.");
    }
}

// ── Event Integration Errors (namespace-level, unchanged) ──
public static class Event
{
    public static Error NotFound(Guid eventId) => Error.NotFound(
        "Event.NotFound",
        $"Event with ID '{eventId}' was not found or not accessible.");

    public static Error InvalidState(string status) => Error.Validation(
        "Event.InvalidState",
        $"Cannot generate post for event in status '{status}'.");
}