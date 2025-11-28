using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Application.Common.ResponseModel;
using MediatR;

namespace Shared.Application.Abstractions.Messaging
{
    public interface ICommand : IRequest<Result>
    {
    }
    public interface ICommand<TResponse> : IRequest<Result<TResponse>>
    {
    }

     public interface IStreamCommand<out TResponse> : IStreamRequest<TResponse>
    {
    }
}
