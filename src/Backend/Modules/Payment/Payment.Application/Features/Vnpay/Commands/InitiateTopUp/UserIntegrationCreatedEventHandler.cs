using Shared.Application.Abstractions.EventBus;
using Microsoft.Extensions.Logging;
using Users.IntegrationEvents;
using Payments.Domain.Repositories;
using Payments.Domain.Entities;
using Payments.Domain.UOW;

namespace Payments.Application.EventHandlers;

public class UserIntegrationCreatedEventHandler 
    : IntegrationEventHandler<UserIntegrationCreatedEvent>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentUnitOfWork _unitOfWork;
    private readonly ILogger<UserIntegrationCreatedEventHandler> _logger;

    public UserIntegrationCreatedEventHandler(
        IWalletRepository walletRepository,
        IPaymentUnitOfWork unitOfWork,
        ILogger<UserIntegrationCreatedEventHandler> logger)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task Handle(
        UserIntegrationCreatedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var userId = integrationEvent.UserId;

        try
        {
            // 1. Check if wallet already exists (IMPORTANT for idempotency)
            var wallet = await _walletRepository
                .GetByUserIdAsync(userId, cancellationToken);

            if (wallet != null)
            {
                _logger.LogInformation(
                    "Wallet already exists for UserId={UserId}, skipping creation",
                    userId);

                return;
            }

            // 2. Create wallet
            wallet = Wallet.Create(userId);

            _walletRepository.Add(wallet);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Wallet created successfully for UserId={UserId}",
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create wallet for UserId={UserId}",
                userId);

            throw; 
        }
    }
}