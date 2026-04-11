using AI.Application.Features.AiPackages.Commands.UpdateAiPackage;
using AI.Application.Features.AiPackages.Dtos;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.AiPackages.Commands;

internal sealed class UpdateAiPackageCommandHandler(
    IAiPackageRepository aiPackageRepository,
    IAiUnitOfWork aiUnitOfWork,
    IValidator<UpdateAiPackageCommand> validator,
    IMapper mapper)
    : ICommandHandler<UpdateAiPackageCommand, AiPackageDto>
{
    public async Task<Result<AiPackageDto>> Handle(
        UpdateAiPackageCommand request,
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

        var duplicate = await aiPackageRepository.ExistsByNameAsync(request.Name, request.Id, cancellationToken);
        if (duplicate)
        {
            return Result.Failure<AiPackageDto>(
                Error.Conflict("AiPackage.DuplicateName", "AI package name already exists."));
        }

        entity.Update(
            request.Name,
            request.Description,
            request.Type,
            request.Price,
            request.TokenQuota,
            request.IsActive);

        await aiUnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<AiPackageDto>(entity));
    }
}
