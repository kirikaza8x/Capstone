using MediatR;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace Shared.Application.Behaviors
{
    public class UnitOfWorkBehavior<TRequest, TResponse> 
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICompositeUnitOfWork _unitOfWork;
        private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

        public UnitOfWorkBehavior(
            ICompositeUnitOfWork unitOfWork,
            ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            var shouldSave = request is ITransactionalCommand;
            
            if (!shouldSave)
            {
                _logger.LogDebug("Skipping auto-save for {RequestName}", typeof(TRequest).Name);
                return await next();
            }

            _logger.LogInformation(" UnitOfWorkBehavior: Executing transactional command {CommandName}", typeof(TRequest).Name);
            
            var response = await next();

            _logger.LogInformation(" UnitOfWorkBehavior: Saving changes for {CommandName}", typeof(TRequest).Name);
            
            try
            {
                var rowsAffected = await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(" UnitOfWorkBehavior: Saved {RowsAffected} rows", rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to save changes for {CommandName}", typeof(TRequest).Name);
                throw;
            }

            return response;
        }
    }
}