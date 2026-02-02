using Microsoft.EntityFrameworkCore;
using Order.Domain.Orders;
using Shared.Infrastructure.Data;

namespace Order.Infrastructure.Data;

public class OrderUnitOfWork : UnitOfWorkBase<OrdersDbContext>, IOrderUnitOfWork
{
    private readonly OrdersDbContext _dbContext;

    public OrderUnitOfWork(OrdersDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
