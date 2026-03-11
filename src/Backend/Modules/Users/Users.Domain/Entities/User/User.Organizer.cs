using Users.Domain.Enums;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public partial class User
{
    private readonly List<OrganizerProfile> _organizerProfiles = new();

    // --------------------
    // Organizer Profiles
    // --------------------
    public virtual IReadOnlyCollection<OrganizerProfile> OrganizerProfiles 
        => _organizerProfiles.AsReadOnly();

    // --------------------
    // Query Helpers
    // --------------------

    // Current live public profile
    public OrganizerProfile? PublishedProfile =>
        _organizerProfiles
            .Where(p => p.Status == OrganizerStatus.Verified)
            .OrderByDescending(p => p.VersionNumber)
            .FirstOrDefault();

    // Profile currently being edited
    public OrganizerProfile? DraftProfile =>
        _organizerProfiles.FirstOrDefault(p => p.Status == OrganizerStatus.Draft);

    // Profile waiting admin approval
    public OrganizerProfile? PendingProfile =>
        _organizerProfiles.FirstOrDefault(p => p.Status == OrganizerStatus.Pending);

    // --------------------
    // Creation
    // --------------------

    public void CreateOrganizerProfile(OrganizerType type)
    {
        if (_organizerProfiles.Any())
            throw new InvalidOperationException("User already has an organizer profile.");

        var profile = OrganizerProfile.Create(Id, type, 1);
        _organizerProfiles.Add(profile);
    }

    // --------------------
    // Update Workflow
    // --------------------

    public void BeginProfileUpdate()
    {
        if (DraftProfile != null || PendingProfile != null)
            throw new InvalidOperationException("A profile update is already in progress.");

        var current = PublishedProfile 
            ?? throw new InvalidOperationException("No verified profile exists to update.");

        int nextVersion = _organizerProfiles.Max(p => p.VersionNumber) + 1;

        var newVersion = OrganizerProfile.CreateNewVersion(current, nextVersion);

        _organizerProfiles.Add(newVersion);
    }

    public void UpdateOrganizerProfile(OrganizerBusinessInfo businessInfo)
    {
        GetDraft().UpdateProfile(businessInfo);
    }

    public void UpdateOrganizerBank(OrganizerBankInfo bankInfo)
    {
        GetDraft().UpdateBankInformation(bankInfo);
    }

    public void SubmitOrganizerProfile()
    {
        GetDraft().SubmitForVerification();
    }

    // --------------------
    // Moderation
    // --------------------

    public void VerifyOrganizerProfile()
    {
        var pending = GetPending();

        // Archive the current verified version
        PublishedProfile?.Archive();

        // Approve new version
        pending.Verify();
    }

    public void RejectOrganizerProfile(string? reason = null)
    {
        GetPending().Reject(reason);
    }

    // --------------------
    // Helpers
    // --------------------

    private OrganizerProfile GetDraft() =>
        DraftProfile ?? throw new InvalidOperationException(
            "No draft profile. Call BeginProfileUpdate first.");

    private OrganizerProfile GetPending() =>
        PendingProfile ?? throw new InvalidOperationException(
            "No pending profile available.");
}