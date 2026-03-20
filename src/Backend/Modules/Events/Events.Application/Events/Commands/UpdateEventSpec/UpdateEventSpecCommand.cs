using System.Text.Json;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateEventSpec;

public sealed record UpdateEventSpecCommand(
    Guid EventId,
    JsonDocument Spec) : ICommand;
