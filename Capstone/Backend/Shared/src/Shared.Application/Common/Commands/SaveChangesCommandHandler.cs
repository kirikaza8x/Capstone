using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Shared.Domain.UnitOfWork;
using Shared.Application.Common.ResponseModel;

namespace Shared.Application.Common.Commands
{
    public sealed class SaveChangesCommandHandler : IRequestHandler<SaveChangesCommand, Result>
    {
        private readonly ICompositeUnitOfWork _unitOfWork;

        public SaveChangesCommandHandler(ICompositeUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(SaveChangesCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}


