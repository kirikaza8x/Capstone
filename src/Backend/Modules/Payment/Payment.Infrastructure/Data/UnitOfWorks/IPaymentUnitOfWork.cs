using Payments.Domain.UOW;
using Payments.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Data;

namespace Payments.Infrastructure.Data.UOW;

public class PaymentUnitOfWork : UnitOfWorkBase<PaymentModuleDbContext>, IPaymentUnitOfWork
{
    public PaymentUnitOfWork(PaymentModuleDbContext dbContext) : base(dbContext)
    {
    }
}
