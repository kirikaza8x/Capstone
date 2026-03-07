using MediatR;
using Shared.Domain.Abstractions;

namespace Shared.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
