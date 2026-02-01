// using MediatR;
// using Microsoft.Extensions.Logging;
// using Shared.Application.Messaging;
// using Shared.Domain.Abstractions;
// using Users.Application.Messaging;
// using Users.Domain.UOW;

// public class UseUnitOfWorkBehavior<TRequest, TResponse> 
//     : IPipelineBehavior<TRequest, TResponse>
//     where TRequest : IUserSaveCommand
//     where TResponse : Result
// {
//     private readonly IUserUnitOfWork _unitOfWork;
//     private readonly ILogger<UseUnitOfWorkBehavior<TRequest, TResponse>> _logger;

//     public UseUnitOfWorkBehavior(
//         IUserUnitOfWork unitOfWork,
//         ILogger<UseUnitOfWorkBehavior<TRequest, TResponse>> logger)
//     {
//         _unitOfWork = unitOfWork;
//         _logger = logger;
//     }

//     public async Task<TResponse> Handle(
//         TRequest request, 
//         RequestHandlerDelegate<TResponse> next, 
//         CancellationToken cancellationToken)
//     {
//         var commandName = typeof(TRequest).Name;
//         _logger.LogInformation("[UserUoW] Processing {Command}", commandName);

//         // Skip queries
//         if (request is IQuery<TResponse>)
//         {
//             _logger.LogInformation("[UserUoW] Skipped {Command} (query)", commandName);
//             return await next();
//         }

//         // Transactional case
//         if (request is ITransactionalUserCommand)
//         {
//             _logger.LogInformation("[UserUoW] Starting transaction for {Command}", commandName);

//             await _unitOfWork.BeginTransactionAsync(cancellationToken);
//             var response = await next();

//             if (response.IsSuccess)
//             {
//                 await _unitOfWork.CommitTransactionAsync(cancellationToken);
//                 _logger.LogInformation("[UserUoW] Transaction committed for {Command}", commandName);
//             }
//             else
//             {
//                 await _unitOfWork.RollbackTransactionAsync(cancellationToken);
//                 _logger.LogWarning("[UserUoW] Transaction rolled back for {Command}", commandName);
//             }

//             return response;
//         }

//         var normalResponse = await next();
//         if (normalResponse.IsSuccess)
//         {
//             await _unitOfWork.SaveChangesAsync(cancellationToken);
//             _logger.LogInformation("[UserUoW] Saved changes for {Command}", commandName);
//         }
//         else
//         {
//             _logger.LogWarning("[UserUoW] No changes saved for {Command} (failure)", commandName);
//         }

//         return normalResponse;
//     }
// }
using Microsoft.Extensions.Logging;
using Shared.Domain.Abstractions;
using Users.Application.Messaging;
using Users.Domain.UOW;

public class UserUnitOfWorkBehavior<TRequest, TResponse> 
    : UnitOfWorkBehaviorBase<TRequest, TResponse, IUserUnitOfWork>
    where TRequest : IUserSaveCommand
    where TResponse : Result
{
    public UserUnitOfWorkBehavior(
        IUserUnitOfWork unitOfWork,
        ILogger<UserUnitOfWorkBehavior<TRequest, TResponse>> logger)
        : base(unitOfWork, logger)
    {
    }
}
