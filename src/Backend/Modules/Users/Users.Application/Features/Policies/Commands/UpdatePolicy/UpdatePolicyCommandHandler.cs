using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Policies.Dtos;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Policies.Commands.UpdatePolicy;

public class UpdatePolicyCommandValidator : AbstractValidator<UpdatePolicyCommand>
{
    public UpdatePolicyCommandValidator()
    {
        RuleFor(x => x.PolicyId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);
    }
}

public sealed class UpdatePolicyCommandHandler : ICommandHandler<UpdatePolicyCommand, PolicyDto>
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IUserUnitOfWork _unitOfWork;
    private readonly IValidator<UpdatePolicyCommand> _validator;
    private readonly IMapper _mapper;

    public UpdatePolicyCommandHandler(
        IPolicyRepository policyRepository,
        IUserUnitOfWork unitOfWork,
        IValidator<UpdatePolicyCommand> validator,
        IMapper mapper)
    {
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result<PolicyDto>> Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var firstError = validation.Errors.First();
            return Result.Failure<PolicyDto>(
                Error.Validation("Policy.Validation", firstError.ErrorMessage));
        }

        var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy is null)
        {
            return Result.Failure<PolicyDto>(
                Error.NotFound("Policy.NotFound", "Policy not found."));
        }

        policy.Update(request.Type, request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<PolicyDto>(policy));
    }
}
