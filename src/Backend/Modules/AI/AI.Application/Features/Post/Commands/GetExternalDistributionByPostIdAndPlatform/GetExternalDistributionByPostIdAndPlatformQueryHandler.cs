using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Marketing.Application.Posts.Dtos;

namespace Marketing.Application.ExternalDistributions.Handlers;

public class GetExternalDistributionByPostIdAndPlatformQueryHandler
    : IQueryHandler<GetExternalDistributionByPostIdAndPlatformQuery, ExternalDistributionDetailDto>
{
    private readonly IExternalDistribuitionRepository _externalDistributionRepository;
    private readonly IMapper _mapper;

    public GetExternalDistributionByPostIdAndPlatformQueryHandler(
        IExternalDistribuitionRepository externalDistributionRepository,
        IMapper mapper)
    {
        _externalDistributionRepository = externalDistributionRepository;
        _mapper = mapper;
    }

    public async Task<Result<ExternalDistributionDetailDto>> Handle(
        GetExternalDistributionByPostIdAndPlatformQuery query,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Fetch aggregate
        // ─────────────────────────────────────────────────────────────
        var distribution = await _externalDistributionRepository.GetByPostIdAndPlatformAsync(query.PostId, query.platForm, cancellationToken);

        if (distribution == null)
        {
            return Result.Failure<ExternalDistributionDetailDto>(
                MarketingErrors.ExternalDistribution.NotFoundByPostAndPlatform(query.PostId, query.platForm));
        }

        // ─────────────────────────────────────────────────────────────
        // Map to DTO
        // ─────────────────────────────────────────────────────────────
        var dto = _mapper.Map<ExternalDistributionDetailDto>(distribution);

        return Result.Success(dto);
    }
}
