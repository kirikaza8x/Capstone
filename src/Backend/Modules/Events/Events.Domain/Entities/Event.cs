using Events.Domain.DomainEvents;
using Events.Domain.Enums;
using Events.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.DDD;
using static Events.Domain.Errors.EventErrors;

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
    public int EventCategoryId { get; private set; }

    public DateTime? TicketSaleStartAt { get; private set; }
    public DateTime? TicketSaleEndAt { get; private set; }
    public DateTime? EventStartAt { get; private set; }
    public DateTime? EventEndAt { get; private set; }

    public string Policy { get; private set; } = string.Empty;
    public string? Spec { get; private set; }
    public string? SeatmapImage { get; private set; }
    public bool IsEmailReminderEnabled { get; private set; } = false;
    public int? EventTypeId { get; private set; }

    private readonly List<EventSession> _sessions = [];
    public IReadOnlyCollection<EventSession> Sessions => _sessions.AsReadOnly();

    private readonly List<EventImage> _images = [];
    public IReadOnlyCollection<EventImage> Images => _images.AsReadOnly();

    private readonly List<EventCategoryMapping> _categoryMappings = [];
    public IReadOnlyCollection<EventCategoryMapping> CategoryMappings => _categoryMappings.AsReadOnly();

    private readonly List<EventStaff> _staffs = [];
    public IReadOnlyCollection<EventStaff> Staffs => _staffs.AsReadOnly();

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
        string description,
        int eventCategoryId)
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
            EventCategoryId = eventCategoryId,
            CreatedAt = DateTime.UtcNow
        };

        @event.RaiseDomainEvent(new EventCreatedDomainEvent(@event.Id, organizerId));

        return @event;
    }

    public void UpdateUrlPath(string urlPath)
    {
        UrlPath = urlPath;
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

    public void UpdateAdditionalInfo(
        string policy,
        string? spec,
        int? eventTypeId)
    {
        Policy = policy;
        Spec = spec;
        EventTypeId = eventTypeId;
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
        {
            UrlPath = urlPath.Trim().ToLowerInvariant();
        }

        ModifiedAt = DateTime.UtcNow;
    }

    public Result Publish()
    {
        if (Status != EventStatus.Draft || Status != EventStatus.Pending)
            return Result.Failure(EventErrors.Event.CannotPublish(Status));

        Status = EventStatus.Published;
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new EventPublishedDomainEvent(Id));

        return Result.Success();
    }

    public Result Unpublish()
    {
        if (Status != EventStatus.Published)
            return Result.Failure(EventErrors.Event.CannotUnpublish(Status));

        Status = EventStatus.Draft;
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new EventUnpublishedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is not (EventStatus.Draft or EventStatus.Published))
            return Result.Failure(EventErrors.Event.CannotCancel(Status));

        Status = EventStatus.Cancelled;
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new EventCancelledDomainEvent(Id));

        return Result.Success();
    }

    public Result CanDelete()
    {
        if (Status != EventStatus.Draft)
            return Result.Failure(EventErrors.Event.CannotDelete(Status));

        return Result.Success();
    }

    public void SetSeatmapImage(string? seatmapImage)
    {
        SeatmapImage = seatmapImage;
        ModifiedAt = DateTime.UtcNow;
    }

    public Result RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            return Result.Failure(EventImageErrors.NotFound(imageId));

        _images.Remove(image);
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public EventImage AddImage(string imageUrl)
    {
        var image = EventImage.Create(Id, imageUrl);
        _images.Add(image);
        ModifiedAt = DateTime.UtcNow;
        return image;
    }

    public Result UpdateImage(Guid imageId, string newImageUrl)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            return Result.Failure(EventImageErrors.NotFound(imageId));

        image.UpdateImageUrl(newImageUrl);
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public EventImage? GetImage(Guid imageId)
    {
        return _images.FirstOrDefault(i => i.Id == imageId);
    }

    public void AddSession(EventSession session) => _sessions.Add(session);
    public void AddArea(Area area) => _areas.Add(area);
    public void AddStaff(EventStaff staff) => _staffs.Add(staff);
    public void AddActorImage(EventActorImage actorImage) => _actorImages.Add(actorImage);
    public void AddCategoryMapping(EventCategoryMapping mapping) => _categoryMappings.Add(mapping);
    public void AddHashtag(EventHashtag eventHashtag) => _eventHashtags.Add(eventHashtag);

    protected override void Apply(IDomainEvent @event)
    { }
}