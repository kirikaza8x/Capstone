using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Policies.Dtos;

namespace Users.Application.Features.Policies.Queries.GetPolicies;

public sealed record GetPoliciesQuery : IQuery<IReadOnlyList<PolicyDto>>;
