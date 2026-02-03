using Shared.Domain.DDD;

public class RecommendationItem : Entity<Guid>
{
    public Guid EventId { get; private set; }
    public double Score { get; private set; }
    public string? Explanation { get; private set; }

    private RecommendationItem() { }

    public RecommendationItem(Guid eventId, double score, string? explanation = null)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Score = score;
        Explanation = explanation;
    }

    // Domain behaviors
    public void AttachExplanation(string explanation) => Explanation = explanation;

    public void Recalculate(double newScore) => Score = newScore;
}