using AI.Domain.Entities;
using AI.Domain.Enums;
using AI.Domain.Interfaces.Repositories;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Payment.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;

namespace AI.Application.Features.AiPackages.EventHandlers;

internal sealed class PaymentSuccessIntegrationEventHandler(
    IAiPackageRepository aiPackageRepository,
    IOrganizerAiQuotaRepository organizerAiQuotaRepository,
    IAiTokenTransactionRepository aiTokenTransactionRepository,
    IAiUnitOfWork aiUnitOfWork,
    ILogger<PaymentSuccessIntegrationEventHandler> logger)
    : IntegrationEventHandler<PaymentSuccessIntegrationEvent>
{
    public override async Task Handle(
        PaymentSuccessIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        if (integrationEvent.ReferenceType != PaymentReferenceType.AiPackage)
            return;

        if (integrationEvent.ReferenceId == Guid.Empty || integrationEvent.UserId == Guid.Empty)
        {
            logger.LogWarning(
                "Invalid AiPackage payment event payload. TxnId={TxnId}, RefId={RefId}, UserId={UserId}",
                integrationEvent.PaymentTransactionId,
                integrationEvent.ReferenceId,
                integrationEvent.UserId);
            return;
        }

        var alreadyProcessed = await aiTokenTransactionRepository
            .ExistsByReferenceIdAsync(integrationEvent.PaymentTransactionId, cancellationToken);

        if (alreadyProcessed)
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

        var quota = await organizerAiQuotaRepository.GetByOrganizerIdAsync(integrationEvent.UserId, cancellationToken);
        if (quota is null)
        {
            quota = OrganizerAiQuota.Create(integrationEvent.UserId);
            organizerAiQuotaRepository.Add(quota);
        }

        var transactionType = package.Type == AiPackageType.Subscription
            ? AiTokenTransactionType.MonthlyGrant
            : AiTokenTransactionType.TopUp;

        if (transactionType == AiTokenTransactionType.MonthlyGrant)
            quota.AddSubscriptionTokens(package.TokenQuota);
        else
            quota.AddTopUpTokens(package.TokenQuota);

        var tokenTxn = AiTokenTransaction.Create(
            quotaId: quota.Id,
            packageId: package.Id,
            type: transactionType,
            amount: package.TokenQuota,
            balanceAfter: quota.TotalTokens,
            referenceId: integrationEvent.PaymentTransactionId);

        aiTokenTransactionRepository.Add(tokenTxn);

        await aiUnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
