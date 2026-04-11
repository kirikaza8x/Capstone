using AI.Domain.Entities;
using AI.Domain.Enums;
using AI.Domain.Interfaces.Repositories;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Payment.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;

namespace AI.Application.Features.AiPackages.EventHandlers;

public sealed class PaymentSuccessIntegrationEventHandler(
    IAiPackageRepository aiPackageRepository,
    IOrganizerAiQuotaRepository organizerAiQuotaRepository,
    IAiTokenTransactionRepository aiTokenTransactionRepository,
    IAiUnitOfWork aiUnitOfWork,
    ILogger<PaymentSuccessIntegrationEventHandler> logger)
    : IntegrationEventHandler<PaymentSuccessIntegrationEvent>
{
    private const int SubscriptionDurationMonths = 1;

    public override async Task Handle(
        PaymentSuccessIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        if (integrationEvent.ReferenceType != PaymentReferenceType.AiPackage)
            return;

        if (IsInvalidPayload(integrationEvent))
            return;

        if (await IsAlreadyProcessedAsync(integrationEvent.PaymentTransactionId, cancellationToken))
            return;

        var package = await aiPackageRepository.GetByIdAsync(integrationEvent.ReferenceId, cancellationToken);
        if (package is null)
        {
            logger.LogError(
                "AiPackage not found for successful payment. TxnId={TxnId}, PackageId={PackageId}",
                integrationEvent.PaymentTransactionId,
                integrationEvent.ReferenceId);
            return;
        }

        var quota = await GetOrCreateQuotaAsync(integrationEvent.UserId, cancellationToken);

        var transactionType = ResolveTransactionType(package.Type);
        var addTokensResult = transactionType == AiTokenTransactionType.MonthlyGrant
            ? quota.AddSubscriptionTokens(
                package.TokenQuota,
                integrationEvent.PaidAtUtc.AddMonths(SubscriptionDurationMonths))
            : quota.AddTopUpTokens(package.TokenQuota);

        if (addTokensResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to add AI tokens. TxnId={TxnId}, UserId={UserId}, Error={Code}-{Description}",
                integrationEvent.PaymentTransactionId,
                integrationEvent.UserId,
                addTokensResult.Error.Code,
                addTokensResult.Error.Description);
            return;
        }

        var tokenTransaction = AiTokenTransaction.Create(
            quotaId: quota.Id,
            packageId: package.Id,
            type: transactionType,
            amount: package.TokenQuota,
            balanceAfter: quota.TotalTokens,
            referenceId: integrationEvent.PaymentTransactionId);

        aiTokenTransactionRepository.Add(tokenTransaction);

        await aiUnitOfWork.SaveChangesAsync(cancellationToken);
    }

    private bool IsInvalidPayload(PaymentSuccessIntegrationEvent integrationEvent)
    {
        if (integrationEvent.ReferenceId != Guid.Empty && integrationEvent.UserId != Guid.Empty)
            return false;

        logger.LogWarning(
            "Invalid AiPackage payment event payload. TxnId={TxnId}, RefId={RefId}, UserId={UserId}",
            integrationEvent.PaymentTransactionId,
            integrationEvent.ReferenceId,
            integrationEvent.UserId);

        return true;
    }

    private async Task<bool> IsAlreadyProcessedAsync(Guid paymentTransactionId, CancellationToken cancellationToken)
    {
        var alreadyProcessed = await aiTokenTransactionRepository.ExistsByReferenceIdAsync(
            paymentTransactionId,
            cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogDebug(
                "Skip duplicate AiPackage payment event. TxnId={TxnId}",
                paymentTransactionId);
        }

        return alreadyProcessed;
    }

    private async Task<OrganizerAiQuota> GetOrCreateQuotaAsync(Guid organizerId, CancellationToken cancellationToken)
    {
        var quota = await organizerAiQuotaRepository.GetByOrganizerIdAsync(organizerId, cancellationToken);
        if (quota is not null)
            return quota;

        quota = OrganizerAiQuota.Create(organizerId);
        organizerAiQuotaRepository.Add(quota);

        return quota;
    }

    private static AiTokenTransactionType ResolveTransactionType(AiPackageType packageType)
    {
        return packageType == AiPackageType.Subscription
            ? AiTokenTransactionType.MonthlyGrant
            : AiTokenTransactionType.TopUp;
    }
}
