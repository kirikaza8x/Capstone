using Shared.Application.Abstractions.Messaging;

namespace Users.Application.Features.Policies.Commands.DeletePolicy;

public sealed record DeletePolicyCommand(Guid PolicyId) : ICommand;
