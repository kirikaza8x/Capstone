using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Marketing.Domain.Repositories;
using Marketing.Application.Posts.Dtos;

namespace Marketing.Application.ExternalDistributions.Handlers;

public class GetExternalDistributionsQueryHandler
    : IQueryHandler<GetExternalDistributionsQuery, PagedResult<ExternalDistributionDto>>
{
    private readonly IExternalDistribuitionRepository _externalDistributionRepository;
    private readonly IMapper _mapper;

    public GetExternalDistributionsQueryHandler(
        IExternalDistribuitionRepository externalDistributionRepository,
        IMapper mapper)
    {
        _externalDistributionRepository = externalDistributionRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ExternalDistributionDto>>> Handle(
        GetExternalDistributionsQuery query,
        CancellationToken cancellationToken)
    {
        var distributions = await _externalDistributionRepository.GetAllWithPagingAsync(
            query,
            d =>
                // ─────────────────────────────
                // Identity
                // ─────────────────────────────
                (query.PostMarketingId == null || d.PostMarketingId == query.PostMarketingId)

                // ─────────────────────────────
                // Platform
                // ─────────────────────────────
                && (query.Platform == null || d.Platform == query.Platform)
                && (string.IsNullOrWhiteSpace(query.ExternalUrl) || d.ExternalUrl.Contains(query.ExternalUrl))
                && (string.IsNullOrWhiteSpace(query.ExternalPostId) || d.ExternalPostId == query.ExternalPostId)

                // ─────────────────────────────
                // Status
                // ─────────────────────────────
                && (query.Status == null || d.Status == query.Status)

                // ─────────────────────────────
                // Metadata
                // ─────────────────────────────
                && (string.IsNullOrWhiteSpace(query.PlatformMetadata) || d.PlatformMetadata!.Contains(query.PlatformMetadata))

                // ─────────────────────────────
                // Error
                // ─────────────────────────────
                && (query.HasError == null ||
                    (query.HasError == true ? d.ErrorMessage != null : d.ErrorMessage == null))

                // ─────────────────────────────
                // Sent Date Range
                // ─────────────────────────────
                && (query.SentFrom == null || (d.SentAt != null && d.SentAt >= query.SentFrom))
                && (query.SentTo == null || (d.SentAt != null && d.SentAt <= query.SentTo)),
            cancellationToken: cancellationToken);

        var dtoItems = _mapper.Map<IReadOnlyList<ExternalDistributionDto>>(distributions.Items);

        return Result.Success(new PagedResult<ExternalDistributionDto>(
            dtoItems,
            distributions.PageNumber,
            distributions.PageSize,
            distributions.TotalCount));
    }
}
