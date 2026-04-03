using AI.Application.Features.AiPackages.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.AiPackages.Queries.GetMyAiQuota;

public sealed record GetMyAiQuotaQuery : IQuery<MyAiQuotaDto>;
