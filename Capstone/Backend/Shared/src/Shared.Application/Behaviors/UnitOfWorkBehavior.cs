using MediatR;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace Shared.Application.Behaviors
{
    public class UnitOfWorkBehavior<TRequest, TResponse> 
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
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
            _logger.LogInformation("Executing command: {CommandName}", typeof(TRequest).Name);
            
            // Execute the command handler
            var response = await next();

            // Auto-save changes after successful command execution
            _logger.LogInformation("Saving changes for command: {CommandName}", typeof(TRequest).Name);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Changes saved successfully for command: {CommandName}", typeof(TRequest).Name);

            return response;
        }
    }
}