namespace Events.PublicApi.Records;
public sealed record EventRecommendationFeature
{
    public Guid Id { get; init; }

    public string Title { get; init; } = default!;
    public string? BannerUrl { get; init; }
    public string Location { get; init; } = default!;

    public DateTime? EventStartAt { get; init; }
    public DateTime? EventEndAt { get; init; }

    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public IReadOnlyCollection<string> Categories { get; init; } = [];
    public IReadOnlyCollection<string> Hashtags { get; init; } = [];
}
