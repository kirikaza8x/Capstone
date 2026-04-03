using AI.Application.Features.AiPackages.Commands.CreateAiPackage;
using AI.Application.Features.AiPackages.Dtos;
using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.AiPackages.Commands;

internal sealed class CreateAiPackageCommandHandler(
    IAiPackageRepository aiPackageRepository,
    IAiUnitOfWork aiUnitOfWork,
    IValidator<CreateAiPackageCommand> validator,
    IMapper mapper)
    : ICommandHandler<CreateAiPackageCommand, AiPackageDto>
{
    public async Task<Result<AiPackageDto>> Handle(
        CreateAiPackageCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AiPackageDto>(
                Error.Validation("AiPackage.Validation", validation.Errors.First().ErrorMessage));
        }

        var exists = await aiPackageRepository.ExistsByNameAsync(request.Name, null, cancellationToken);
        if (exists)
        {
            return Result.Failure<AiPackageDto>(
                Error.Conflict("AiPackage.DuplicateName", "AI package name already exists."));
        }

        var entity = AiPackage.Create(
            request.Name,
            request.Description,
            request.Type,
            request.Price,
            request.TokenQuota);

        aiPackageRepository.Add(entity);
        await aiUnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<AiPackageDto>(entity));
    }
}
