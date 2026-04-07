using AI.Application.Abstractions;
using AI.Domain.Entities;
using AI.Domain.Enums;
using AI.Domain.Interfaces.Repositories;
using AI.Domain.Repositories;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;

namespace AI.Application.Services;

public sealed class AiTokenQuotaService(
    IOrganizerAiQuotaRepository quotaRepository,
    IAiTokenTransactionRepository tokenTransactionRepository,
    IDateTimeProvider dateTimeProvider) : IAiTokenQuotaService
{
    public async Task<Result> ConsumeAsync(
        Guid organizerId,
        int tokens,
        Guid? referenceId = null,
        CancellationToken ct = default)
    {
        if (tokens <= 0)
        {
            return Result.Failure(Error.Validation(
                "AiQuota.TokenAmountMustBePositive",
                "Token amount must be greater than zero."));
        }

        var quota = await quotaRepository.GetByOrganizerIdAsync(organizerId, ct);
        if (quota is null)
        {
            return Result.Failure(Error.NotFound(
                "AiQuota.NotFound",
                $"AI quota for organizer '{organizerId}' was not found."));
        }

        AddExpiredTransactionIfNeeded(quota, referenceId);

        var consumeResult = quota.ConsumeTokens(tokens);
        if (consumeResult.IsFailure)
            return Result.Failure(consumeResult.Error);

        var usageTransaction = AiTokenTransaction.Create(
            quotaId: quota.Id,
            packageId: null,
            type: AiTokenTransactionType.UsagePost,
            amount: tokens,
            balanceAfter: quota.TotalTokens,
            referenceId: referenceId);

        tokenTransactionRepository.Add(usageTransaction);

        return Result.Success();
    }

    private void AddExpiredTransactionIfNeeded(OrganizerAiQuota quota, Guid? referenceId)
    {
        var expiredTokens = quota.ExpireSubscriptionTokens(dateTimeProvider.UtcNow);
        if (expiredTokens <= 0)
            return;

        var expiredTransaction = AiTokenTransaction.Create(
            quotaId: quota.Id,
            packageId: null,
            type: AiTokenTransactionType.Expired,
            amount: expiredTokens,
            balanceAfter: quota.TotalTokens,
            referenceId: referenceId);

        tokenTransactionRepository.Add(expiredTransaction);
    }
}
