using AI.Application.Features.AiPackages.Dtos;
using AI.Application.Features.AiPackages.Queries.GetAiPackageById;
using AI.Domain.Repositories;
using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.AiPackages.Queries;

internal sealed class GetAiPackageByIdQueryHandler(
    IAiPackageRepository aiPackageRepository,
    IMapper mapper)
    : IQueryHandler<GetAiPackageByIdQuery, AiPackageDto>
{
    public async Task<Result<AiPackageDto>> Handle(
        GetAiPackageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var item = await aiPackageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            return Result.Failure<AiPackageDto>(
                Error.NotFound("AiPackage.NotFound", "AI package was not found."));
        }

        return Result.Success(mapper.Map<AiPackageDto>(item));
    }
}
