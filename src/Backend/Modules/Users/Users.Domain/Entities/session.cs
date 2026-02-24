using Shared.Domain.DDD;

namespace Users.Domain.Entities
{
    public class UserSession : Entity<Guid>
    {
        public Guid SessionId { get; private set; }
        public Guid UserId { get; private set; }
        public string DeviceType { get; private set; } = default!;
        public string Source { get; private set; } = default!;
        public Guid? CampaignId { get; private set; }
        public DateTime LastActiveAt { get; private set; }

        // EF Core constructor
        private UserSession() { }

        public static UserSession Create(
            Guid userId,
            string deviceType,
            string source,
            Guid? campaignId = null)
        {
            var session = new UserSession
            {
                SessionId = Guid.NewGuid(),
                UserId = userId,
                DeviceType = deviceType,
                Source = source,
                CampaignId = campaignId,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };

            return session;
        }

        // Domain behaviors
        public void UpdateLastActive()
        {
            LastActiveAt = DateTime.UtcNow;
        }

        public void AttachCampaign(Guid campaignId)
        {
            CampaignId = campaignId;
        }
    }
}
