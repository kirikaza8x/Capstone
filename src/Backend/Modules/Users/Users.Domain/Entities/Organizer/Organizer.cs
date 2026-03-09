using System;
using Shared.Domain.DDD;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public partial class OrganizerProfile : Entity<Guid>
    {
        // --------------------
        // Identity
        // --------------------
        public Guid UserId { get; private set; }

        // --------------------
        // Profile
        // --------------------
        public string? Logo { get; private set; }
        public string? DisplayName { get; private set; }
        public string? Description { get; private set; }
        public string? Address { get; private set; }
        public string? SocialLink { get; private set; }

        // --------------------
        // Business
        // --------------------
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
        // Status
        // --------------------
        public OrganizerStatus Status { get; private set; }
        public DateTimeOffset? VerifiedAt { get; private set; }
        public OrganizerType Type { get; private set; }

        // --------------------
        // Methods
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
            Logo = logo ?? Logo;
            DisplayName = displayName ?? DisplayName;
            Description = description ?? Description;
            Address = address ?? Address;
            SocialLink = socialLink ?? SocialLink;
            BusinessType = businessType ?? BusinessType;
            TaxCode = taxCode ?? TaxCode;
            IdentityNumber = identityNumber ?? IdentityNumber;
            CompanyName = companyName ?? CompanyName;
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
        }

        public void ChangeStatus(OrganizerStatus status, DateTimeOffset? verifiedAt = null)
        {
            Status = status;
            VerifiedAt = verifiedAt;
        }

        public void SetType(OrganizerType type)
        {
            Type = type;
        }
    }
}
