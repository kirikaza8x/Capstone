using Shared.Application.Common.ResponseModel;
using MediatR;

namespace Shared.Application.Abstractions.Messaging
{
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    {
    }

}
