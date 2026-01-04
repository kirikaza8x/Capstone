using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Data;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Shared.Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatic transaction management
/// </summary>
internal sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITransactionalCommand)
        {
            return await next();
        }

        var commandName = typeof(TRequest).Name;

        logger.LogDebug("Beginning transaction for {CommandName}", commandName);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            TResponse result = await next();

            if (result.IsSuccess)
            {
                logger.LogDebug("Committing transaction for {CommandName}", commandName);
                await unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            else
            {
                logger.LogWarning(
                    "Rolling back transaction for {CommandName} due to business failure: {ErrorCode}",
                    commandName,
                    result.Error.Code);

                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Rolling back transaction for {CommandName} due to exception",
                commandName);

            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
