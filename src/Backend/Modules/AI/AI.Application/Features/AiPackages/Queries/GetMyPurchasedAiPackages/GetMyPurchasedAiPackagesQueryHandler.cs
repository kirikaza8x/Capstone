using AI.Application.Features.AiPackages.Dtos;
using AI.Application.Features.AiPackages.Queries.GetMyPurchasedAiPackages;
using AI.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.AiPackages.Queries;

internal sealed class GetMyPurchasedAiPackagesQueryHandler(
    ICurrentUserService currentUserService,
    IOrganizerAiQuotaRepository organizerAiQuotaRepository,
    IAiTokenTransactionRepository aiTokenTransactionRepository)
    : IQueryHandler<GetMyPurchasedAiPackagesQuery, IReadOnlyList<MyPurchasedAiPackageDto>>
{
    public async Task<Result<IReadOnlyList<MyPurchasedAiPackageDto>>> Handle(
        GetMyPurchasedAiPackagesQuery request,
        CancellationToken cancellationToken)
    {
        var organizerId = currentUserService.UserId;
        if (organizerId == Guid.Empty)
        {
            return Result.Failure<IReadOnlyList<MyPurchasedAiPackageDto>>(Error.Unauthorized(
                "AI.Unauthorized",
                "Current user is not authenticated."));
        }

        var quota = await organizerAiQuotaRepository.GetByOrganizerIdAsync(organizerId, cancellationToken);
        if (quota is null)
        {
            return Result.Success<IReadOnlyList<MyPurchasedAiPackageDto>>([]);
        }

        var transactions = await aiTokenTransactionRepository.GetPurchasedByQuotaIdAsync(quota.Id, cancellationToken);

        var data = transactions
            .GroupBy(x => x.Package!)
            .Select(g => new MyPurchasedAiPackageDto(
                g.Key.Id,
                g.Key.Name,
                g.Key.Description,
                g.Key.Type,
                g.Key.Price,
                g.Key.TokenQuota,
                g.Key.IsActive,
                g.Count(),
                g.Sum(x => x.Amount),
                g.Max(x => x.CreatedAt ?? DateTime.MinValue)))
            .OrderByDescending(x => x.LastPurchasedAt)
            .ToList();

        return Result.Success<IReadOnlyList<MyPurchasedAiPackageDto>>(data);
    }
}
