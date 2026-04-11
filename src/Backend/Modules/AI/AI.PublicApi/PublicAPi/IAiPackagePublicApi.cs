namespace AI.PublicApi.PublicApi;

public interface IAiPackagePublicApi
{
    Task<AiPackagePaymentInfoDto?> GetPackageForPaymentAsync(
        Guid packageId,
        CancellationToken cancellationToken = default);

    Task<AiPackageBasicInfoDto?> GetPackageBasicInfoAsync(
        Guid packageId,
        CancellationToken cancellationToken = default);
}

public sealed record AiPackagePaymentInfoDto(
    Guid Id,
    decimal Price,
    bool IsActive);

public sealed record AiPackageBasicInfoDto(
    Guid Id,
    string Name,
    bool IsActive);
