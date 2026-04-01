namespace Users.PublicApi.Records;

public sealed record UserMetricsDto(
    int TotalAttendees,
    int TotalOrganizers,
    double UserGrowthRate);
