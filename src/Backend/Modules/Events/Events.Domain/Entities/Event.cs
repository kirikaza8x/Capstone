using Events.Domain.DomainEvents;
using Events.Domain.Enums;
using Events.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class Event : AggregateRoot<Guid>
{
    public Guid OrganizerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public EventStatus Status { get; private set; }
    public string? BannerUrl { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public string? MapUrl { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? UrlPath { get; private set; }
    public DateTime? TicketSaleStartAt { get; private set; }
    public DateTime? TicketSaleEndAt { get; private set; }
    public DateTime? EventStartAt { get; private set; }
    public DateTime? EventEndAt { get; private set; }
    public string Policy { get; private set; } = string.Empty;
    public string? Spec { get; private set; }
    public bool IsEmailReminderEnabled { get; private set; } = false;
    public DateTime? ReminderTriggeredAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? PublishRejectionReason { get; private set; }
    public string? CancellationRejectionReason { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateTime? SuspendedAt { get; private set; }
    public DateTime? SuspendedUntilAt { get; private set; }
    public Guid? SuspendedBy { get; private set; }

    private readonly List<EventSession> _sessions = [];
    public IReadOnlyCollection<EventSession> Sessions => _sessions.AsReadOnly();

    private readonly List<TicketType> _ticketTypes = [];
    public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes.AsReadOnly();

    private readonly List<EventImage> _images = [];
    public IReadOnlyCollection<EventImage> Images => _images.AsReadOnly();

    private readonly List<EventCategory> _eventCategories = [];
    public IReadOnlyCollection<EventCategory> EventCategories => _eventCategories.AsReadOnly();

    private readonly List<EventMember> _members = [];
    public IReadOnlyCollection<EventMember> Members => _members.AsReadOnly();

    private readonly List<Area> _areas = [];
    public IReadOnlyCollection<Area> Areas => _areas.AsReadOnly();

    private readonly List<EventActorImage> _actorImages = [];
    public IReadOnlyCollection<EventActorImage> ActorImages => _actorImages.AsReadOnly();

    private readonly List<EventHashtag> _eventHashtags = [];
    public IReadOnlyCollection<EventHashtag> EventHashtags => _eventHashtags.AsReadOnly();

    private Event() { }

    public static Event Create(
        Guid organizerId,
        string title,
        string? bannerUrl,
        string location,
        string? mapUrl,
        string description)
    {
        var @event = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            Title = title,
            Status = EventStatus.Draft,
            BannerUrl = bannerUrl,
            Location = location,
            MapUrl = mapUrl,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        @event.RaiseDomainEvent(new EventCreatedDomainEvent(@event.Id, organizerId));
        var categoryNames = @event.EventCategories
        .Select(ec => ec.Category.Name)
        .ToList();

        var hashtagNames = @event.EventHashtags
            .Select(eh => eh.Hashtag.Name)
            .ToList();

        // @event.RaiseDomainEvent(new EventCreatedEmbeddingDomainEvent(
        //     @event.Id,
        //     organizerId,
        //     @event.Title,
        //     @event.Description,
        //     @event.GetCategoryNames(),
        //     @event.GetHashtagNames(),
        //     @event.IsActive,
        //     @event.CreatedAt ?? DateTime.UtcNow
        // ));
        return @event;
    }

    public void UpdateInfo(string title, string location, string? mapUrl, string description)
    {
        Title = title;
        Location = location;
        MapUrl = mapUrl;
        Description = description;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateUrlPath(string urlPath)
    {
        UrlPath = urlPath;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdatePolicy(string policy)
    {
        Policy = policy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSchedule(
        DateTime ticketSaleStartAt,
        DateTime ticketSaleEndAt,
        DateTime eventStartAt,
        DateTime eventEndAt)
    {
        TicketSaleStartAt = ticketSaleStartAt;
        TicketSaleEndAt = ticketSaleEndAt;
        EventStartAt = eventStartAt;
        EventEndAt = eventEndAt;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateBannerUrl(string? bannerUrl)
    {
        BannerUrl = bannerUrl;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSettings(bool isEmailReminderEnabled, string? urlPath)
    {
        IsEmailReminderEnabled = isEmailReminderEnabled;
        if (!string.IsNullOrWhiteSpace(urlPath))
            UrlPath = urlPath.Trim().ToLowerInvariant();
        ModifiedAt = DateTime.UtcNow;
    }

    public void ReplaceHashtags(IEnumerable<EventHashtag> hashtags)
    {
        _eventHashtags.Clear();
        _eventHashtags.AddRange(hashtags);
        ModifiedAt = DateTime.UtcNow;
    }

    public void ReplaceCategories(IEnumerable<EventCategory> categories)
    {
        _eventCategories.Clear();
        _eventCategories.AddRange(categories);
        ModifiedAt = DateTime.UtcNow;
    }

    public void ReplaceActorImages(IEnumerable<EventActorImage> actorImages)
    {
        _actorImages.Clear();
        _actorImages.AddRange(actorImages);
        ModifiedAt = DateTime.UtcNow;
    }

    public Result Publish()
    {
        if (Status is not (EventStatus.Draft or EventStatus.PendingReview))
            return Result.Failure(EventErrors.Event.CannotPublish(Status));

        if (EventStartAt is null || EventEndAt is null || TicketSaleStartAt is null || TicketSaleEndAt is null)
            return Result.Failure(EventErrors.Event.MissingSchedule);

        if (TicketSaleEndAt >= EventEndAt)
            return Result.Failure(EventErrors.Event.InvalidTicketSalePeriod);

        if (_sessions.Count == 0)
            return Result.Failure(EventErrors.Event.NoSessions);

        if (_ticketTypes.Count == 0)
            return Result.Failure(EventErrors.Event.NoTicketTypes);

        if (_ticketTypes.Any(t => t.AreaId is null))
            return Result.Failure(EventErrors.Event.TicketTypeNotAssignedToArea);

        Status = EventStatus.Published;
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new EventPublishedDomainEvent(Id));
        // RaiseDomainEvent(new EventPublishedEmbeddingDomainEvent(
        //     Id,
        //     OrganizerId,
        //     Title,
        //     Description,
        //     GetCategoryNames(),
        //     GetHashtagNames(),
        //     IsActive,
        //     ModifiedAt ?? DateTime.UtcNow
        // ));
        return Result.Success();
    }

    public Result Suspend(Guid suspendedBy, string reason, TimeSpan fixWindow, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;

        if (Status != EventStatus.Published)
            return Result.Failure(EventErrors.Event.CannotSuspend(Status));

        if (!EventStartAt.HasValue)
            return Result.Failure(EventErrors.Event.MissingSchedule);

        if (EventStartAt.Value <= now)
            return Result.Failure(EventErrors.Event.CannotSuspendAfterStart);

        if (suspendedBy == Guid.Empty)
            return Result.Failure(EventErrors.Event.SuspendByRequired);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(EventErrors.Event.SuspendReasonRequired);

        if (fixWindow <= TimeSpan.Zero)
            return Result.Failure(EventErrors.Event.InvalidSuspendFixWindow);

        Status = EventStatus.Suspended;
        SuspendedBy = suspendedBy;
        SuspensionReason = reason.Trim();
        SuspendedAt = now;
        SuspendedUntilAt = now.Add(fixWindow);
        ModifiedAt = now;

        RaiseDomainEvent(new EventSuspendedDomainEvent(
            EventId: Id,
            OrganizerId: OrganizerId,
            SuspendedBy: suspendedBy,
            EventTitle: Title,
            SuspensionReason: SuspensionReason,
            SuspendedUntilAtUtc: SuspendedUntilAt.Value));

        return Result.Success();
    }

    public Result Cancel(string? reason = null)
    {
        if (Status is not (EventStatus.Suspended or EventStatus.PendingCancellation))
            return Result.Failure(EventErrors.Event.CannotCancel(Status));

        Status = EventStatus.Cancelled;

        if (!string.IsNullOrWhiteSpace(reason))
            CancellationReason = reason;

        ModifiedAt = DateTime.UtcNow;
        RaiseDomainEvent(new EventCancelledDomainEvent(Id, CancellationReason));
        return Result.Success();
    }

    public Result CanDelete()
    {
        if (Status != EventStatus.Draft)
            return Result.Failure(EventErrors.Event.CannotDelete(Status));
        return Result.Success();
    }

    public Result RequestCancellation(string reason)
    {
        if (Status != EventStatus.Published)
            return Result.Failure(EventErrors.Event.CannotRequestCancellation(Status));

        if (EventStartAt.HasValue && EventStartAt.Value <= DateTime.UtcNow)
            return Result.Failure(EventErrors.Event.AlreadyStarted);

        Status = EventStatus.PendingCancellation;
        CancellationReason = reason;
        CancellationRejectionReason = null;
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RequestPublish(DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;

        if (Status is not (EventStatus.Draft or EventStatus.Suspended))
            return Result.Failure(EventErrors.Event.CannotRequestPublish(Status));

        if (Status == EventStatus.Suspended && SuspendedUntilAt.HasValue && now > SuspendedUntilAt.Value)
            return Result.Failure(EventErrors.Event.CannotResubmitAfterSuspendDeadline);

        Status = EventStatus.PendingReview;
        PublishRejectionReason = null;
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            return Result.Failure(EventErrors.EventImageErrors.NotFound(imageId));

        _images.Remove(image);
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateImage(Guid imageId, string newImageUrl)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            return Result.Failure(EventErrors.EventImageErrors.NotFound(imageId));

        image.UpdateImageUrl(newImageUrl);
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public EventImage? GetImage(Guid imageId) => _images.FirstOrDefault(i => i.Id == imageId);

    public void RemoveSession(EventSession session)
    {
        _sessions.Remove(session);
        ModifiedAt = DateTime.UtcNow;
    }

    public EventImage AddImage(string imageUrl)
    {
        var image = EventImage.Create(Id, imageUrl);
        _images.Add(image);
        ModifiedAt = DateTime.UtcNow;
        return image;
    }

    public void UpdateSpec(string spec)
    {
        Spec = spec;
        ModifiedAt = DateTime.UtcNow;
    }

    public void ClearAreasAndSeats()
    {
        _areas.Clear();
        ModifiedAt = DateTime.UtcNow;
    }

    public void RemoveMember(EventMember member)
    {
        _members.Remove(member);
        ModifiedAt = DateTime.UtcNow;
    }

    public void AddSession(EventSession session) => _sessions.Add(session);
    public void AddTicketType(TicketType ticketType) => _ticketTypes.Add(ticketType);
    public void AddArea(Area area) => _areas.Add(area);
    public void AddMember(EventMember member) => _members.Add(member);
    public void AddActorImage(EventActorImage actorImage) => _actorImages.Add(actorImage);
    public void AddCategories(EventCategory eventCategory) => _eventCategories.Add(eventCategory);
    public void AddHashtag(EventHashtag eventHashtag) => _eventHashtags.Add(eventHashtag);
    public void RemoveTicketType(TicketType ticketType)
    {
        _ticketTypes.Remove(ticketType);
        ModifiedAt = DateTime.UtcNow;
    }

    public Result Complete(DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;

        if (Status != EventStatus.Published)
            return Result.Failure(EventErrors.Event.CannotComplete(Status));

        if (EventEndAt is null || EventEndAt > now)
            return Result.Failure(EventErrors.Event.CannotCompleteBeforeEnd);

        Status = EventStatus.Completed;
        ModifiedAt = now;

        RaiseDomainEvent(new EventCompletedDomainEvent(Id));
        return Result.Success();
    }

    public Result MarkReminderTriggered(DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;

        if (ReminderTriggeredAt.HasValue)
            return Result.Success();

        if (Status != EventStatus.Published)
            return Result.Failure(EventErrors.Event.CannotTriggerReminder(Status));

        if (!IsEmailReminderEnabled)
            return Result.Failure(EventErrors.Event.EmailReminderDisabled);

        if (!EventStartAt.HasValue)
            return Result.Failure(EventErrors.Event.MissingSchedule);

        var startAt = EventStartAt.Value;
        var dueAt = startAt.AddHours(-24);

        if (now < dueAt || now >= startAt)
            return Result.Failure(EventErrors.Event.ReminderNotDue);

        ReminderTriggeredAt = now;
        ModifiedAt = now;

        RaiseDomainEvent(new EventReminderTriggeredDomainEvent(
            EventId: Id,
            EventTitle: Title,
            EventStartAtUtc: startAt));

        return Result.Success();
    }

    public Result RejectPublishRequest(string reason)
    {
        if (Status != EventStatus.PendingReview)
            return Result.Failure(EventErrors.Event.CannotRejectPublishRequest(Status));

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(EventErrors.Event.RejectReasonRequired);

        Status = EventStatus.Draft;
        PublishRejectionReason = reason.Trim();
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RejectCancellationRequest(string reason)
    {
        if (Status != EventStatus.PendingCancellation)
            return Result.Failure(EventErrors.Event.CannotRejectCancellationRequest(Status));

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(EventErrors.Event.RejectReasonRequired);

        Status = EventStatus.Published;
        CancellationRejectionReason = reason.Trim();
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }


    /// <summary>
    /// Returns the list of category names for this event.
    /// </summary>
    public List<string> GetCategoryNames()
    {
        return EventCategories
            .Select(ec => ec.Category.Name)
            .ToList();
    }

    /// <summary>
    /// Returns the list of hashtag names for this event.
    /// </summary>
    public List<string> GetHashtagNames()
    {
        return EventHashtags
            .Select(eh => eh.Hashtag.Name)
            .ToList();
    }
    protected override void Apply(IDomainEvent @event) { }
}