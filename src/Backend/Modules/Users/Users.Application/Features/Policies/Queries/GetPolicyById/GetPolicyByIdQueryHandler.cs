using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Policies.Dtos;
using Users.Application.Features.Policies.Queries.GetPolicyById;
using Users.Domain.Repositories;

namespace Users.Application.Features.Policies.Queries;

public sealed class GetPolicyByIdQueryHandler : IQueryHandler<GetPolicyByIdQuery, PolicyDto>
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IMapper _mapper;

    public GetPolicyByIdQueryHandler(IPolicyRepository policyRepository, IMapper mapper)
    {
        _policyRepository = policyRepository;
        _mapper = mapper;
    }

    public async Task<Result<PolicyDto>> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);

        if (policy is null)
        {
            return Result.Failure<PolicyDto>(
                Error.NotFound("Policy.NotFound", "Policy not found."));
        }

        return Result.Success(_mapper.Map<PolicyDto>(policy));
    }
}
