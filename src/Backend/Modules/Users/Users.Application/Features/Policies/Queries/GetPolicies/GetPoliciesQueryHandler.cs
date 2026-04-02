using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Policies.Dtos;
using Users.Application.Features.Policies.Queries.GetPolicies;
using Users.Domain.Repositories;

namespace Users.Application.Features.Policies.Queries;

public sealed class GetPoliciesQueryHandler : IQueryHandler<GetPoliciesQuery, IReadOnlyList<PolicyDto>>
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IMapper _mapper;

    public GetPoliciesQueryHandler(IPolicyRepository policyRepository, IMapper mapper)
    {
        _policyRepository = policyRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<PolicyDto>>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
    {
        var items = await _policyRepository.GetListAsync(cancellationToken);
        var data = _mapper.Map<IReadOnlyList<PolicyDto>>(items);
        return Result.Success(data);
    }
}
