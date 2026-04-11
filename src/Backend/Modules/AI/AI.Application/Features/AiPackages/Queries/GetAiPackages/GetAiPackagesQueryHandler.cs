using AI.Application.Features.AiPackages.Dtos;
using AI.Application.Features.AiPackages.Queries.GetAiPackages;
using AI.Domain.Repositories;
using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.AiPackages.Queries;

internal sealed class GetAiPackagesQueryHandler(
    IAiPackageRepository aiPackageRepository,
    IMapper mapper)
    : IQueryHandler<GetAiPackagesQuery, IReadOnlyList<AiPackageDto>>
{
    public async Task<Result<IReadOnlyList<AiPackageDto>>> Handle(
        GetAiPackagesQuery request,
        CancellationToken cancellationToken)
    {
        var items = await aiPackageRepository.GetListAsync(cancellationToken);
        var response = mapper.Map<IReadOnlyList<AiPackageDto>>(items);
        return Result.Success(response);
    }
}
