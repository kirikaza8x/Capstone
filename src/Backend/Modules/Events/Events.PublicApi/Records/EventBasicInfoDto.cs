namespace Events.PublicApi.Records;

public sealed record EventBasicInfoDto(
    Guid Id,
    string Title,
    string BannerUrl,
    string Status);
