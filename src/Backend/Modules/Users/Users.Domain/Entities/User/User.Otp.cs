using Shared.Domain.DDD;
using Users.Domain.Errors.Otp;
using Users.Domain.Events;

namespace Users.Domain.Entities
{
    public partial class User : AggregateRoot<Guid>
    {
        private readonly List<Otp> _otps = new();
        public IReadOnlyCollection<Otp> Otps => _otps.AsReadOnly();

        // --------------------
        // Password Reset Flow
        // --------------------

        public Otp CreateOtp()
        {
            // Invalidate any previous unused OTPs to prevent confusion
            foreach (var existingOtp in _otps.Where(o => !o.IsUsed))
            {
                existingOtp.MarkUsed();
            }

            var otp = Otp.Create(Id);
            _otps.Add(otp);

            RaiseDomainEvent(new OtpCreatedEvent(Id, otp.OtpCode));
            return otp;
        }

        private Otp? GetLatestActiveOtp() =>
            _otps.Where(o => !o.IsUsed && !o.IsExpired())
                 .OrderByDescending(o => o.CreatedAt)
                 .FirstOrDefault();

        public bool VerifyOtp(string otpCode)
        {
            var otp = GetLatestActiveOtp();
            return otp != null && otp.Verify(otpCode);
        }

        public void ResetPassword(string otpCode, string newPasswordHash)
        {
            var otp = GetLatestActiveOtp();

            if (otp == null || !otp.Verify(otpCode))
            {
                throw new InvalidOperationException(OtpErrors.InvalidCode.Code);
            }

            ChangePassword(newPasswordHash);
            otp.MarkUsed();

            RaiseDomainEvent(new PasswordChangedEvent(Id));
        }


    }
}