namespace AI.PublicApi.PublicApi;

public interface IAiPackagePublicApi
{
    Task<AiPackagePaymentInfoDto?> GetPackageForPaymentAsync(
        Guid packageId,
        CancellationToken cancellationToken = default);
}

public sealed record AiPackagePaymentInfoDto(
    Guid Id,
    decimal Price,
    bool IsActive);
