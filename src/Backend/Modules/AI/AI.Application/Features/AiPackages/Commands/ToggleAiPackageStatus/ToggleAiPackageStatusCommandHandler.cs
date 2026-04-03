using AI.Application.Features.AiPackages.Commands.ToggleAiPackageStatus;
using AI.Application.Features.AiPackages.Dtos;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.AiPackages.Commands;

internal sealed class ToggleAiPackageStatusCommandHandler(
    IAiPackageRepository aiPackageRepository,
    IAiUnitOfWork aiUnitOfWork,
    IValidator<ToggleAiPackageStatusCommand> validator,
    IMapper mapper)
    : ICommandHandler<ToggleAiPackageStatusCommand, AiPackageDto>
{
    public async Task<Result<AiPackageDto>> Handle(
        ToggleAiPackageStatusCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AiPackageDto>(
                Error.Validation("AiPackage.Validation", validation.Errors.First().ErrorMessage));
        }

        var entity = await aiPackageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<AiPackageDto>(
                Error.NotFound("AiPackage.NotFound", "AI package was not found."));
        }

        entity.ToggleStatus();

        await aiUnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<AiPackageDto>(entity));
    }
}
