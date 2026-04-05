using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Policies.Dtos;

namespace Users.Application.Features.Policies.Commands.UpdatePolicy;

public sealed record UpdatePolicyCommand(
    Guid PolicyId,
    string Type,
    string? FileUrl,
    string Description) : ICommand<PolicyDto>;
