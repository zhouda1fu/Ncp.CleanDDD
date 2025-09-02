using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.OrderAggregate;
using NetCorePal.Extensions.Repository;

namespace Ncp.CleanDDD.Infrastructure.Repositories;

public interface IOrderRepository : IRepository<Order, OrderId>
{
}

public class OrderRepository(ApplicationDbContext context) : RepositoryBase<Order, OrderId, ApplicationDbContext>(context), IOrderRepository
{
}

