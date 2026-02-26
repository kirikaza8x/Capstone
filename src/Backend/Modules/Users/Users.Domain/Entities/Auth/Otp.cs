using Shared.Domain.DDD;

namespace Users.Domain.Entities
{
    public class Otp : Entity<Guid>
    {
        public Guid UserId { get; private set; }
        public string OtpCode { get; private set; } = default!;
        public DateTime ExpiryDate { get; private set; }
        public bool IsUsed { get; private set; }

        private Otp() { }

        public static Otp Create(Guid userId)
        {
            var otp = GenerateOtp();
            return new Otp
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OtpCode = otp,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };
        }

        public bool Verify(string otpCode) => OtpCode == otpCode && !IsExpired();
        public bool IsExpired() => DateTime.UtcNow > ExpiryDate;
        public void MarkUsed() => IsUsed = true;

        private static string GenerateOtp()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString(); 
        }
    }
    
}
