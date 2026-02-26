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
            var otp = Otp.Create(Id);
            _otps.Add(otp);

            RaiseDomainEvent(new OtpCreatedEvent(Id, otp.OtpCode));
            return otp;
        }

        public bool VerifyOtp(string otpCode)
        {
            var otp = _otps
                .Where(o => !o.IsUsed && !o.IsExpired())
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            return otp != null && otp.Verify(otpCode);
        }

        public void ResetPassword(string otpCode, string newPasswordHash)
        {
            var otp = _otps
                .Where(o => !o.IsUsed && !o.IsExpired())
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (otp == null || !otp.Verify(otpCode))
                throw new InvalidOperationException(OtpErrors.InvalidCode.ToString());

            ChangePassword(newPasswordHash);
            otp.MarkUsed();

            RaiseDomainEvent(new PasswordChangedEvent(Id));
        }
    }

    
}
