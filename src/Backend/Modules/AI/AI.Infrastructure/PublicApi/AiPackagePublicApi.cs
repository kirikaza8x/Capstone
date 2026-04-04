using AI.Infrastructure.Data;
using AI.PublicApi.PublicApi;
using Microsoft.EntityFrameworkCore;

namespace AI.Infrastructure.PublicApi;

internal sealed class AiPackagePublicApi(AIModuleDbContext dbContext) : IAiPackagePublicApi
{
    public async Task<AiPackagePaymentInfoDto?> GetPackageForPaymentAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AiPackages
            .AsNoTracking()
            .Where(x => x.Id == packageId)
            .Select(x => new AiPackagePaymentInfoDto(
                x.Id,
                x.Price,
                x.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
