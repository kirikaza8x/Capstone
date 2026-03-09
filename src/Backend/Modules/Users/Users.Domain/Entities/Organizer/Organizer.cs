using System;
using Shared.Domain.DDD;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public partial class OrganizerProfile : Entity<Guid>
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
        // EF Constructor
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

        public void UpdateProfile(
            string? logo,
            string? displayName,
            string? description,
            string? address,
            string? socialLink,
            BusinessType? businessType,
            string? taxCode,
            string? identityNumber,
            string? companyName)
        {
            if (Status == OrganizerStatus.Rejected)
                throw new InvalidOperationException("Cannot update rejected organizer profile.");

            Logo = logo ?? Logo;
            DisplayName = displayName ?? DisplayName;
            Description = description ?? Description;
            Address = address ?? Address;
            SocialLink = socialLink ?? SocialLink;

            if (businessType.HasValue)
                BusinessType = businessType.Value;

            TaxCode = taxCode ?? TaxCode;
            IdentityNumber = identityNumber ?? IdentityNumber;
            CompanyName = companyName ?? CompanyName;

            ModifiedAt = DateTime.UtcNow;
        }

        public void UpdateBankInformation(
            string? accountName,
            string? accountNumber,
            string? bankCode,
            string? branch)
        {
            AccountName = accountName ?? AccountName;
            AccountNumber = accountNumber ?? AccountNumber;
            BankCode = bankCode ?? BankCode;
            Branch = branch ?? Branch;

            ModifiedAt = DateTime.UtcNow;
        }

        public void SubmitForVerification()
        {
            if (Status != OrganizerStatus.Draft)
                throw new InvalidOperationException("Only draft organizers can be submitted.");

            Status = OrganizerStatus.Pending;
            ModifiedAt = DateTime.UtcNow;
        }

        public void Verify()
        {
            if (Status != OrganizerStatus.Pending)
                throw new InvalidOperationException("Organizer must be pending verification.");

            VerifiedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        public void Reject(string? reason = null)
        {
            Status = OrganizerStatus.Rejected;
            ModifiedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            Status = OrganizerStatus.Suspended;
            ModifiedAt = DateTime.UtcNow;
        }

        public bool IsVerified()
        {
            return Status == OrganizerStatus.Verified;
        }
    }
}