using System;
using Shared.Domain.DDD;
using Users.Domain.Enums;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities
{
    public class OrganizerProfile : Entity<Guid>
    {
        // --------------------
        // Identity
        // --------------------
        public Guid UserId { get; private set; }
        public virtual User User { get; private set; } = null!;

        // --------------------
        // Versioning
        // --------------------
        public int VersionNumber { get; private set; }

        // --------------------
        // Profile Info
        // --------------------
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

        // --------------------
        // Bank
        // --------------------
        public string? AccountName { get; private set; }
        public string? AccountNumber { get; private set; }
        public string? BankCode { get; private set; }
        public string? Branch { get; private set; }

        // --------------------
        // Status
        // --------------------
        public OrganizerStatus Status { get; private set; }
        public DateTimeOffset? VerifiedAt { get; private set; }

        public OrganizerType Type { get; private set; }

        private OrganizerProfile() { }

        // --------------------
        // Factory
        // --------------------

        internal static OrganizerProfile Create(Guid userId, OrganizerType type, int version)
        {
            return new OrganizerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                VersionNumber = version,
                Status = OrganizerStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        internal static OrganizerProfile CreateWithDetails(
            Guid userId, 
            OrganizerType type, 
            int version, 
            OrganizerBusinessInfo businessInfo, 
            OrganizerBankInfo bankInfo)
        {
            var profile = Create(userId, type, version);

            profile.UpdateProfile(businessInfo);
            profile.UpdateBankInformation(bankInfo);

            return profile;
        }
        internal static OrganizerProfile CreateNewVersion(OrganizerProfile current, int nextVersion)
        {
            var profile = Create(current.UserId, current.Type, nextVersion);

            profile.Logo = current.Logo;
            profile.DisplayName = current.DisplayName;
            profile.Description = current.Description;
            profile.Address = current.Address;
            profile.SocialLink = current.SocialLink;

            profile.BusinessType = current.BusinessType;
            profile.TaxCode = current.TaxCode;
            profile.IdentityNumber = current.IdentityNumber;
            profile.CompanyName = current.CompanyName;

            profile.AccountName = current.AccountName;
            profile.AccountNumber = current.AccountNumber;
            profile.BankCode = current.BankCode;
            profile.Branch = current.Branch;

            return profile;
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

        public void UpdateBankInformation(OrganizerBankInfo bank)
        {
            EnsureDraft();

            AccountName = bank.AccountName ?? AccountName;
            AccountNumber = bank.AccountNumber ?? AccountNumber;
            BankCode = bank.BankCode ?? BankCode;
            Branch = bank.Branch ?? Branch;

        }

        public void SubmitForVerification()
        {
            EnsureDraft();
            // if (string.IsNullOrWhiteSpace(DisplayName))
            //     throw new InvalidOperationException("Display name required.");

            // if (string.IsNullOrWhiteSpace(AccountNumber))
            //     throw new InvalidOperationException("Bank account required.");

            Status = OrganizerStatus.Pending;
        }

        public void Verify()
        {
            if (Status != OrganizerStatus.Pending)
                throw new InvalidOperationException("Profile must be pending.");

            Status = OrganizerStatus.Verified;
            VerifiedAt = DateTimeOffset.UtcNow;
            RejectionReason = null;

        }

        public void Reject(string? reason)
        {
            if (Status != OrganizerStatus.Pending)
                throw new InvalidOperationException("Only pending profiles can be rejected.");

            Status = OrganizerStatus.Rejected;
            RejectionReason = reason;

        }

        public void Archive()
        {
            if (Status != OrganizerStatus.Verified)
                throw new InvalidOperationException("Only verified profiles can be archived.");

            Status = OrganizerStatus.Archived;
        }

        private void EnsureDraft()
        {
            if (Status != OrganizerStatus.Draft)
                throw new InvalidOperationException("Changes allowed only in Draft.");
        }
    }
}
