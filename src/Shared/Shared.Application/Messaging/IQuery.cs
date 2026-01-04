using MediatR;
using Shared.Domain.Abstractions;

namespace Shared.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
