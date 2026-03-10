using Users.Domain.Enums;
using Users.Domain.ValueObjects;

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

        OrganizerProfile = OrganizerProfile.Create(Id, type);
    }

    /// <summary>
    /// Update organizer business information.
    /// </summary>
    public void UpdateOrganizerProfile(OrganizerBusinessInfo businessInfo)
    {
        var organizer = GetOrganizer();

        organizer.UpdateProfile(businessInfo);
    }

    /// <summary>
    /// Update organizer bank information.
    /// </summary>
    public void UpdateOrganizerBank(OrganizerBankInfo bankInfo)
    {
        var organizer = GetOrganizer();

        organizer.UpdateBankInformation(bankInfo);
    }

    /// <summary>
    /// Submit organizer profile for verification.
    /// </summary>
    public void SubmitOrganizerProfile()
    {
        var organizer = GetOrganizer();

        organizer.SubmitForVerification();
    }

    /// <summary>
    /// Admin verifies organizer.
    /// </summary>
    public void VerifyOrganizerProfile()
    {
        var organizer = GetOrganizer();

        organizer.Verify();

        // Domain Event example
        // AddDomainEvent(new OrganizerProfileVerifiedEvent(Id, organizer.Id));
    }

    /// <summary>
    /// Admin rejects organizer.
    /// </summary>
    public void RejectOrganizerProfile(string? reason = null)
    {
        var organizer = GetOrganizer();

        organizer.Reject(reason);

        // AddDomainEvent(new OrganizerProfileRejectedEvent(Id, reason));
    }

    /// <summary>
    /// Admin requests organizer changes.
    /// </summary>
    public void RequestOrganizerChanges()
    {
        var organizer = GetOrganizer();

        organizer.RequestChanges();
    }

    /// <summary>
    /// Suspend organizer account.
    /// </summary>
    public void SuspendOrganizer()
    {
        var organizer = GetOrganizer();

        organizer.Deactivate();
    }

    // --------------------
    // Helper
    // --------------------

    private OrganizerProfile GetOrganizer()
    {
        if (OrganizerProfile == null)
            throw new InvalidOperationException("User does not have an organizer profile.");

        return OrganizerProfile;
    }
}