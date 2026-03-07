using MediatR;
using Shared.Domain.Abstractions;

namespace Shared.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

public interface IStreamCommand<out TResponse> : IStreamRequest<TResponse>
{
}