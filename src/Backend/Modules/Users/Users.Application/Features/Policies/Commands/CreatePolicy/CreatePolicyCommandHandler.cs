using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Policies.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Policies.Commands.CreatePolicy;

public class CreatePolicyCommandValidator : AbstractValidator<CreatePolicyCommand>
{
    public CreatePolicyCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.FileUrl)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.FileUrl));

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);
    }
}

public sealed class CreatePolicyCommandHandler : ICommandHandler<CreatePolicyCommand, PolicyDto>
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IUserUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePolicyCommand> _validator;
    private readonly IMapper _mapper;

    public CreatePolicyCommandHandler(
        IPolicyRepository policyRepository,
        IUserUnitOfWork unitOfWork,
        IValidator<CreatePolicyCommand> validator,
        IMapper mapper)
    {
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result<PolicyDto>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var firstError = validation.Errors.First();
            return Result.Failure<PolicyDto>(
                Error.Validation("Policy.Validation", firstError.ErrorMessage));
        }

        var policy = Policy.Create(request.Type, request.FileUrl, request.Description);

        _policyRepository.Add(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<PolicyDto>(policy));
    }
}
