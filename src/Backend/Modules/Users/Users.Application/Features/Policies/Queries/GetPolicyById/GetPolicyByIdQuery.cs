using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Policies.Dtos;

namespace Users.Application.Features.Policies.Queries.GetPolicyById;

public sealed record GetPolicyByIdQuery(Guid PolicyId) : IQuery<PolicyDto>;
