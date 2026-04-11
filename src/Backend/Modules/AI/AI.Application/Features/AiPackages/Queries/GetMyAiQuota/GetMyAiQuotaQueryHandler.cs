using AI.Application.Features.AiPackages.Dtos;
using AI.Application.Features.AiPackages.Queries.GetMyAiQuota;
using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.AiPackages.Queries;

internal sealed class GetMyAiQuotaQueryHandler(
    ICurrentUserService currentUserService,
    IOrganizerAiQuotaRepository organizerAiQuotaRepository,
    IAiUnitOfWork aiUnitOfWork,
    IMapper mapper)
    : IQueryHandler<GetMyAiQuotaQuery, MyAiQuotaDto>
{
    public async Task<Result<MyAiQuotaDto>> Handle(GetMyAiQuotaQuery request, CancellationToken cancellationToken)
    {
        var organizerId = currentUserService.UserId;
        if (organizerId == Guid.Empty)
        {
            return Result.Failure<MyAiQuotaDto>(Error.Unauthorized(
                "AI.Unauthorized",
                "Current user is not authenticated."));
        }

        var quota = await organizerAiQuotaRepository.GetByOrganizerIdAsync(organizerId, cancellationToken);
        if (quota is null)
        {
            quota = OrganizerAiQuota.Create(organizerId);
            organizerAiQuotaRepository.Add(quota);
            await aiUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(mapper.Map<MyAiQuotaDto>(quota));
    }
}
