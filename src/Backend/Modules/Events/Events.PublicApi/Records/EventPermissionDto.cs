namespace Events.PublicApi.Records;

public sealed record EventPermissionDto(
    Guid OrganizerId,
    List<string>? MemberPermissions);
