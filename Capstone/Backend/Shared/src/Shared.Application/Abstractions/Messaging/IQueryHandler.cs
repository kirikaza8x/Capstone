using Shared.Application.Common.ResponseModel;
using MediatR;

namespace Shared.Application.Abstractions.Messaging
{
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>
    {
    }

}
