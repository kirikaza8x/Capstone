using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Data;
using System.Diagnostics;

public abstract class UnitOfWorkBehaviorBase<TRequest, TResponse, TUnitOfWork> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
    where TUnitOfWork : IUnitOfWork
{
    private readonly TUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    protected UnitOfWorkBehaviorBase(TUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;
        var correlationId = Guid.NewGuid().ToString("N");
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("[UoW:{CorrelationId}] Starting {Command} at {StartTime}",
            correlationId, commandName, DateTime.UtcNow);

        try
        {
            if (request is IQuery<TResponse>)
            {
                _logger.LogInformation("[UoW:{CorrelationId}] Skipped {Command} (query)", correlationId, commandName);
                return await next();
            }

            if (request is ITransactionalCommand)
            {
                _logger.LogInformation("[UoW:{CorrelationId}] Beginning transaction for {Command}", correlationId, commandName);

                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                var response = await next();

                if (response.IsSuccess)
                {
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    _logger.LogInformation("[UoW:{CorrelationId}] Transaction committed for {Command} (Elapsed: {Elapsed} ms)",
                        correlationId, commandName, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning("[UoW:{CorrelationId}] Transaction rolled back for {Command} (Error: {Error}, Elapsed: {Elapsed} ms)",
                        correlationId, commandName, response.Error?.Description, stopwatch.ElapsedMilliseconds);
                }

                return response;
            }

            var normalResponse = await next();
            if (normalResponse.IsSuccess)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("[UoW:{CorrelationId}] Saved changes for {Command} (Elapsed: {Elapsed} ms)",
                    correlationId, commandName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("[UoW:{CorrelationId}] No changes saved for {Command} (Failure: {Error}, Elapsed: {Elapsed} ms)",
                    correlationId, commandName, normalResponse.Error?.Description, stopwatch.ElapsedMilliseconds);
            }

            return normalResponse;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("[UoW:{CorrelationId}] Finished {Command} at {EndTime} (Total: {Elapsed} ms)",
                correlationId, commandName, DateTime.UtcNow, stopwatch.ElapsedMilliseconds);
        }
    }
}
