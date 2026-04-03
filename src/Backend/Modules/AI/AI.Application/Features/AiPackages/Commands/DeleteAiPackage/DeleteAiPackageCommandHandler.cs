using AI.Application.Features.AiPackages.Commands.DeleteAiPackage;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.AiPackages.Commands;

internal sealed class DeleteAiPackageCommandHandler(
    IAiPackageRepository aiPackageRepository,
    IAiUnitOfWork aiUnitOfWork)
    : ICommandHandler<DeleteAiPackageCommand>
{
    public async Task<Result> Handle(DeleteAiPackageCommand request, CancellationToken cancellationToken)
    {
        var entity = await aiPackageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure(
                Error.NotFound("AiPackage.NotFound", "AI package was not found."));
        }

        aiPackageRepository.Remove(entity);
        await aiUnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
