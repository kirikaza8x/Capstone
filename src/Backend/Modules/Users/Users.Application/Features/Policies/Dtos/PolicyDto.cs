namespace Users.Application.Features.Policies.Dtos;

public sealed record PolicyDto(
    Guid Id,
    string Type,
    string Description);
