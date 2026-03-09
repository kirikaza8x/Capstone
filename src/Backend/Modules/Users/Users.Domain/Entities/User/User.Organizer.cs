using Users.Domain.Enums;

namespace Users.Domain.Entities;

public partial class User
{
    // --------------------
    // Organizer Profile
    // --------------------

    public virtual OrganizerProfile? OrganizerProfile { get; private set; }

    /// <summary>
    /// Create organizer profile for the user.
    /// Aggregate Root controls entity creation.
    /// </summary>
    public void CreateOrganizerProfile(OrganizerType type)
    {
        if (OrganizerProfile != null)
            throw new InvalidOperationException("User already has an organizer profile.");

        OrganizerProfile = OrganizerProfile.Create(this.Id, type);
    }

    /// <summary>
    /// Update organizer business information.
    /// </summary>
    public void UpdateOrganizerProfile(
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
        EnsureOrganizerExists();

        OrganizerProfile!.UpdateProfile(
            logo,
            displayName,
            description,
            address,
            socialLink,
            businessType,
            taxCode,
            identityNumber,
            companyName
        );
    }

    /// <summary>
    /// Update organizer bank information.
    /// </summary>
    public void UpdateOrganizerBank(
        string? accountName,
        string? accountNumber,
        string? bankCode,
        string? branch)
    {
        EnsureOrganizerExists();

        OrganizerProfile!.UpdateBankInformation(
            accountName,
            accountNumber,
            bankCode,
            branch
        );
    }

    /// <summary>
    /// Submit organizer for verification.
    /// </summary>
    public void SubmitOrganizerProfile()
    {
        EnsureOrganizerExists();

        OrganizerProfile!.SubmitForVerification();
    }

    /// <summary>
    /// Admin verification step.
    /// </summary>
    public void VerifyOrganizerProfile()
    {
        EnsureOrganizerExists();

        OrganizerProfile!.Verify();

        // Example Domain Event
        // AddDomainEvent(new OrganizerProfileVerifiedEvent(this.Id, OrganizerProfile.Id));
    }

    /// <summary>
    /// Admin rejects organizer.
    /// </summary>
    public void RejectOrganizerProfile(string? reason = null)
    {
        EnsureOrganizerExists();

        OrganizerProfile!.Reject(reason);

        // AddDomainEvent(new OrganizerProfileRejectedEvent(this.Id, reason));
    }

    /// <summary>
    /// Suspend organizer account.
    /// </summary>
    public void SuspendOrganizer()
    {
        EnsureOrganizerExists();

        OrganizerProfile!.Deactivate();
    }

    // --------------------
    // Helper
    // --------------------

    private void EnsureOrganizerExists()
    {
        if (OrganizerProfile == null)
            throw new InvalidOperationException("User does not have an organizer profile.");
    }
}