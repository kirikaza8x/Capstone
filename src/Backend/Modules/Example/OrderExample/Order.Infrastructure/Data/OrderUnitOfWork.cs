using Shared.Infrastructure.Data;
using Order.Domain.Orders;

namespace Order.Infrastructure.Data;

public class OrderUnitOfWork : UnitOfWorkBase<OrdersDbContext>, IOrderUnitOfWork
{
    public OrderUnitOfWork(OrdersDbContext dbContext) : base(dbContext)
    {
    }
}
