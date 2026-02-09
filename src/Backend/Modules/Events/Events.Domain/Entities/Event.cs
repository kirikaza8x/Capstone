using Events.Domain.DomainEvents;
using Events.Domain.Enums;
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
    public string UrlPath { get; private set; } = string.Empty;
    public int EventCategoryId { get; private set; }

    public DateTime? TicketSaleStartAt { get; private set; }
    public DateTime? TicketSaleEndAt { get; private set; }
    public DateTime? EventStartAt { get; private set; }
    public DateTime? EventEndAt { get; private set; }

    public string Policy { get; private set; } = string.Empty;
    public string? Spec { get; private set; }
    public string? SeatmapImage { get; private set; }
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
        string urlPath,
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
            UrlPath = urlPath,
            EventCategoryId = eventCategoryId,
            CreatedAt = DateTime.UtcNow
        };

        @event.RaiseDomainEvent(new EventCreatedDomainEvent(@event.Id, organizerId));

        return @event;
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

    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published.");

        Status = EventStatus.Published;
        ModifiedAt = DateTime.UtcNow;

        RaiseDomainEvent(new EventPublishedDomainEvent(Id));
    }

    public void Close()
    {
        if (Status != EventStatus.Published)
            throw new InvalidOperationException("Only published events can be closed.");

        Status = EventStatus.Closed;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetSeatmapImage(string? seatmapImage)
    {
        SeatmapImage = seatmapImage;
        ModifiedAt = DateTime.UtcNow;
    }

    public void AddSession(EventSession session) => _sessions.Add(session);
    public void AddImage(EventImage image) => _images.Add(image);
    public void AddArea(Area area) => _areas.Add(area);
    public void AddStaff(EventStaff staff) => _staffs.Add(staff);
    public void AddActorImage(EventActorImage actorImage) => _actorImages.Add(actorImage);
    public void AddCategoryMapping(EventCategoryMapping mapping) => _categoryMappings.Add(mapping);
    public void AddHashtag(EventHashtag eventHashtag) => _eventHashtags.Add(eventHashtag);


    protected override void Apply(IDomainEvent @event)
    { }
}