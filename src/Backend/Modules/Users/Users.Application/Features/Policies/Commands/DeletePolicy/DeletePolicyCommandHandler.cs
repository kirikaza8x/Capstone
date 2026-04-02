using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Policies.Commands.DeletePolicy;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Policies.Commands;

public sealed class DeletePolicyCommandHandler : ICommandHandler<DeletePolicyCommand>
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IUserUnitOfWork _unitOfWork;

    public DeletePolicyCommandHandler(
        IPolicyRepository policyRepository,
        IUserUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeletePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Result.Failure(
                Error.NotFound("Policy.NotFound", "Policy not found."));
        }

        _policyRepository.Remove(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
