using Shared.Application.Common.ResponseModel;
using MediatR;

namespace Shared.Application.Abstractions.Messaging
{
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result> where TCommand : ICommand
    {
    }

    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>> where TCommand : ICommand<TResponse>
    {
    }

    public interface IStreamCommandHandler<TCommand, TResponse> : IStreamRequestHandler<TCommand, TResponse>
        where TCommand : IStreamCommand<TResponse>
    {
    }
}
