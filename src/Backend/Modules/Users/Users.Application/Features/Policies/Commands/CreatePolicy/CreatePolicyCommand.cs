using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Policies.Dtos;

namespace Users.Application.Features.Policies.Commands.CreatePolicy;

public sealed record CreatePolicyCommand(
    string Type,
    string Description) : ICommand<PolicyDto>;
