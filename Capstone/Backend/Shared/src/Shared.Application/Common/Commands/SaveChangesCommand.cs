using MediatR;
using Shared.Application.Common.ResponseModel;

namespace Shared.Application.Common.Commands
{
    public sealed record SaveChangesCommand() : IRequest<Result>;
}


