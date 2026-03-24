using Users.Domain.Enums;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public partial class User
{
    private readonly List<OrganizerProfile> _organizerProfiles = new();

    public IReadOnlyCollection<OrganizerProfile> OrganizerProfiles => _organizerProfiles;

    public OrganizerProfile? DraftProfile =>
        _organizerProfiles.FirstOrDefault(x => x.Status == OrganizerStatus.Draft);

    public OrganizerProfile? PendingProfile =>
        _organizerProfiles.FirstOrDefault(x => x.Status == OrganizerStatus.Pending);

    public OrganizerProfile? PublishedProfile =>
        _organizerProfiles
            .Where(x => x.Status == OrganizerStatus.Verified)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefault();

    // --------------------
    // MAIN ENTRY 
    // --------------------
    public void StartOrUpdateOrganizerProfile(
        OrganizerType type,
        OrganizerBusinessInfo business,
        OrganizerBankInfo bank)
    {
        if (DraftProfile != null)
        {
            DraftProfile.UpdateProfile(business);
            DraftProfile.UpdateBank(bank);
            return;
        }

        if (PendingProfile != null)
            throw new InvalidOperationException("Under review.");

        var rejected = _organizerProfiles
            .FirstOrDefault(x => x.Status == OrganizerStatus.Rejected);

        if (rejected != null)
        {
            rejected.Reopen();
            rejected.UpdateProfile(business);
            rejected.UpdateBank(bank);
            return;
        }

        if (PublishedProfile != null)
        {
            BeginUpdate();
            DraftProfile!.UpdateProfile(business);
            DraftProfile.UpdateBank(bank);
            return;
        }

        var profile = OrganizerProfile.CreateWithDetails(Id, type, 1, business, bank);
        _organizerProfiles.Add(profile);
    }

    public void BeginUpdate()
    {
        if (DraftProfile != null || PendingProfile != null)
            throw new InvalidOperationException("Already editing.");

        var current = PublishedProfile
            ?? throw new InvalidOperationException("No verified profile.");

        int version = _organizerProfiles.Max(x => x.VersionNumber) + 1;

        _organizerProfiles.Add(
            OrganizerProfile.CreateNewVersion(current, version));
    }

    public void SubmitProfile() => GetDraft().Submit();

    public void VerifyProfile()
    {
        var pending = GetPending();
        PublishedProfile?.Archive();
        pending.Verify();
    }

    public void UpdateDraftLogo(string logo)
    {
        var draft = GetDraft(); // already protected
        draft.UpdateLogo(logo);
    }

    public void RejectProfile(string? reason) =>
        GetPending().Reject(reason);

    private OrganizerProfile GetDraft() =>
        DraftProfile ?? throw new InvalidOperationException("No draft.");

    private OrganizerProfile GetPending() =>
        PendingProfile ?? throw new InvalidOperationException("No pending.");
}