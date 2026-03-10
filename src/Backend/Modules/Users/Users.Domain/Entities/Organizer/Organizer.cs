using System;
using Shared.Domain.DDD;
using Users.Domain.Enums;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities
{
    public class OrganizerProfile : Entity<Guid>
    {
        // --------------------
        // Identity & Navigation
        // --------------------
        public Guid UserId { get; private set; }
        public virtual User User { get; private set; } = null!;

        // --------------------
        // Profile & Business Info
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

        // --------------------
        // Bank Information
        // --------------------
        public string? AccountName { get; private set; }
        public string? AccountNumber { get; private set; }
        public string? BankCode { get; private set; }
        public string? Branch { get; private set; }

        // --------------------
        // Status & Type
        // --------------------
        public OrganizerStatus Status { get; private set; }
        public DateTimeOffset? VerifiedAt { get; private set; }
        public OrganizerType Type { get; private set; }

        // --------------------
        // EF Core Constructor
        // --------------------
        private OrganizerProfile() { }

        // --------------------
        // Factory (DDD)
        // --------------------
        internal static OrganizerProfile Create(Guid userId, OrganizerType type)
        {
            return new OrganizerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Status = OrganizerStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        // --------------------
        // Domain Behaviors
        // --------------------

        public void UpdateProfile(OrganizerBusinessInfo info)
        {
            if (Status != OrganizerStatus.Draft)
                throw new InvalidOperationException(
                    "Organizer profile can only be edited in draft state."
                );

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

            ModifiedAt = DateTime.UtcNow;
        }

        public void UpdateBankInformation(OrganizerBankInfo bankInfo)
        {
            if (Status != OrganizerStatus.Draft)
                throw new InvalidOperationException(
                    "Bank information can only be updated in draft state."
                );

            AccountName = bankInfo.AccountName ?? AccountName;
            AccountNumber = bankInfo.AccountNumber ?? AccountNumber;
            BankCode = bankInfo.BankCode ?? BankCode;
            Branch = bankInfo.Branch ?? Branch;

        }

        public void SubmitForVerification()
        {
            if (Status != OrganizerStatus.Draft)
                throw new InvalidOperationException(
                    "Only draft organizers can be submitted for verification."
                );

            Status = OrganizerStatus.Pending;
            ModifiedAt = DateTime.UtcNow;
        }

        public void Verify()
        {
            if (Status != OrganizerStatus.Pending)
                throw new InvalidOperationException(
                    "Organizer must be pending verification."
                );

            Status = OrganizerStatus.Verified;
            VerifiedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        public void Reject(string? reason = null)
        {
            if (Status != OrganizerStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending organizers can be rejected."
                );

            Status = OrganizerStatus.Rejected;
            ModifiedAt = DateTime.UtcNow;
        }

        public void RequestChanges()
        {
            if (Status != OrganizerStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending organizers can request changes."
                );

            Status = OrganizerStatus.Draft;
            ModifiedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (Status != OrganizerStatus.Verified)
                throw new InvalidOperationException(
                    "Only verified organizers can be suspended."
                );

            Status = OrganizerStatus.Suspended;
            ModifiedAt = DateTime.UtcNow;
        }

        public bool IsVerified()
        {
            return Status == OrganizerStatus.Verified;
        }
    }
}