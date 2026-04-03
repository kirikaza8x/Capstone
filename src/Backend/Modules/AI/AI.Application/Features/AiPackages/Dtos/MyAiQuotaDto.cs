namespace AI.Application.Features.AiPackages.Dtos;

public sealed record MyAiQuotaDto(
    Guid OrganizerId,
    int SubscriptionTokens,
    int TopUpTokens,
    int TotalTokens);
