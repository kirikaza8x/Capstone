using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Domain.Data;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Shared.Application.Behaviors;

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
        if (request is not ICommand)
        {
            return await next();
        }

        var commandName = typeof(TRequest).Name; 

        if (request is ITransactionalCommand)
        {
            return await HandleWithTransactionAsync(
                request,
                next,
                commandName,
                cancellationToken);
        }

        return await HandleWithSaveChangesAsync(
            request,
            next,
            commandName,
            cancellationToken);
    }

    private async Task<TResponse> HandleWithTransactionAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        string commandName,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "[Transaction] Beginning explicit transaction for {CommandName}",
            commandName);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            TResponse result = await next();

            if (result.IsSuccess)
            {
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                logger.LogDebug(
                    "[Transaction] Committed transaction for {CommandName}",
                    commandName);
            }
            else
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);

                logger.LogWarning(
                    "[Transaction] Rolled back transaction for {CommandName} due to: {ErrorCode}",
                    commandName,
                    result.Error.Code);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[Transaction] Rolling back transaction for {CommandName} due to exception",
                commandName);

            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<TResponse> HandleWithSaveChangesAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        string commandName,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("[SaveChanges] Processing command {CommandName}",
            commandName);

        try
        {
            TResponse result = await next();

            if (result.IsSuccess)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogDebug("[SaveChanges] Saved changes for {CommandName}", commandName);
            }
            else
            {
                logger.LogWarning("[SaveChanges] Skipped SaveChanges for {CommandName} due to: {ErrorCode}", commandName, result.Error.Code);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[SaveChanges] Error in {CommandName}, changes not saved",
                commandName);
            throw;
        }
    }
}
