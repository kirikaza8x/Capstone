using Shared.Domain.DDD;
using Users.Domain.Enums;

namespace Users.Domain.ValueObjects;

public sealed class OrganizerBusinessInfo : ValueObject
{
    public string? Logo { get; }
    public string? DisplayName { get; }
    public string? Description { get; }
    public string? Address { get; }
    public string? SocialLink { get; }
    public BusinessType? BusinessType { get; }
    public string? TaxCode { get; }
    public string? IdentityNumber { get; }
    public string? CompanyName { get; }

    public OrganizerBusinessInfo(
        string? displayName,
        string? description,
        string? address,
        string? socialLink,
        BusinessType? businessType,
        string? taxCode,
        string? identityNumber,
        string? companyName)
    {
        DisplayName = displayName;
        Description = description;
        Address = address;
        SocialLink = socialLink;
        BusinessType = businessType;
        TaxCode = taxCode;
        IdentityNumber = identityNumber;
        CompanyName = companyName;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Logo;
        yield return DisplayName;
        yield return Description;
        yield return Address;
        yield return SocialLink;
        yield return BusinessType;
        yield return TaxCode;
        yield return IdentityNumber;
        yield return CompanyName;
    }
}
