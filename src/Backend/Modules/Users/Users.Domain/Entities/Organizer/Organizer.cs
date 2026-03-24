using Shared.Domain.DDD;
using Users.Domain.Enums;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities
{
    public class OrganizerProfile : Entity<Guid>
    {
        public Guid UserId { get; private set; }
        public int VersionNumber { get; private set; }

        public string? Logo { get; private set; }
        public string? DisplayName { get; private set; }
        public string? Description { get; private set; }
        public string? Address { get; private set; }
        public string? SocialLink { get; private set; }

        public BusinessType BusinessType { get; private set; }
        public string? TaxCode { get; private set; }
        public string? IdentityNumber { get; private set; }
        public string? CompanyName { get; private set; }

        public string? RejectionReason { get; private set; }

        public string? AccountName { get; private set; }
        public string? AccountNumber { get; private set; }
        public string? BankCode { get; private set; }
        public string? Branch { get; private set; }

        public OrganizerStatus Status { get; private set; }
        public DateTimeOffset? VerifiedAt { get; private set; }

        public OrganizerType Type { get; private set; }
        public User User { get; private set; } = null!;
        private OrganizerProfile() { }

        // --------------------
        // Factory
        // --------------------
        internal static OrganizerProfile CreateWithDetails(
            Guid userId,
            OrganizerType type,
            int version,
            OrganizerBusinessInfo business,
            OrganizerBankInfo bank)
        {
            return new OrganizerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                VersionNumber = version,
                Status = OrganizerStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,

                Logo = business.Logo,
                DisplayName = business.DisplayName,
                Description = business.Description,
                Address = business.Address,
                SocialLink = business.SocialLink,
                BusinessType = business.BusinessType ?? default,
                TaxCode = business.TaxCode,
                IdentityNumber = business.IdentityNumber,
                CompanyName = business.CompanyName,

                AccountName = bank.AccountName,
                AccountNumber = bank.AccountNumber,
                BankCode = bank.BankCode,
                Branch = bank.Branch
            };
        }

        internal static OrganizerProfile CreateNewVersion(OrganizerProfile current, int version)
        {
            return new OrganizerProfile
            {
                Id = Guid.NewGuid(),
                UserId = current.UserId,
                Type = current.Type,
                VersionNumber = version,
                Status = OrganizerStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,

                Logo = current.Logo,
                DisplayName = current.DisplayName,
                Description = current.Description,
                Address = current.Address,
                SocialLink = current.SocialLink,
                BusinessType = current.BusinessType,
                TaxCode = current.TaxCode,
                IdentityNumber = current.IdentityNumber,
                CompanyName = current.CompanyName,

                AccountName = current.AccountName,
                AccountNumber = current.AccountNumber,
                BankCode = current.BankCode,
                Branch = current.Branch
            };
        }

        // --------------------
        // Behaviors
        // --------------------
        public void UpdateProfile(OrganizerBusinessInfo info)
        {
            EnsureDraft();

            Logo = info.Logo ?? Logo;
            DisplayName = info.DisplayName ?? DisplayName;
            Description = info.Description ?? Description;
            Address = info.Address ?? Address;
            SocialLink = info.SocialLink ?? SocialLink;

            if (info.BusinessType.HasValue)
                BusinessType = info.BusinessType.Value;

            TaxCode = info.TaxCode ?? TaxCode;
            IdentityNumber = info.IdentityNumber ?? IdentityNumber;
            CompanyName = info.CompanyName ?? CompanyName;
        }

        public void UpdateBank(OrganizerBankInfo bank)
        {
            EnsureDraft();

            AccountName = bank.AccountName ?? AccountName;
            AccountNumber = bank.AccountNumber ?? AccountNumber;
            BankCode = bank.BankCode ?? BankCode;
            Branch = bank.Branch ?? Branch;
        }

        public void Submit()
        {
            EnsureDraft();
            Status = OrganizerStatus.Pending;
        }

        public void Verify()
        {
            if (Status != OrganizerStatus.Pending)
                throw new InvalidOperationException("Must be pending.");

            Status = OrganizerStatus.Verified;
            VerifiedAt = DateTimeOffset.UtcNow;
            RejectionReason = null;
        }

        public void Reject(string? reason)
        {
            if (Status != OrganizerStatus.Pending)
                throw new InvalidOperationException("Must be pending.");

            Status = OrganizerStatus.Rejected;
            RejectionReason = reason;
        }

        public void Reopen()
        {
            if (Status != OrganizerStatus.Rejected)
                throw new InvalidOperationException("Only rejected can reopen.");

            Status = OrganizerStatus.Draft;
            RejectionReason = null;
        }

        public void Archive()
        {
            if (Status != OrganizerStatus.Verified)
                throw new InvalidOperationException("Only verified can archive.");

            Status = OrganizerStatus.Archived;
        }

        private void EnsureDraft()
        {
            if (Status != OrganizerStatus.Draft)
                throw new InvalidOperationException("Only draft editable.");
        }
        public void UpdateLogo(string logo)
        {
            EnsureDraft();
            Logo = logo;
        }
    }
}
