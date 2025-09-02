using Ncp.CleanDDD.Domain;
using Ncp.CleanDDD.Domain.AggregatesModel.OrderAggregate;
using Ncp.CleanDDD.Infrastructure;
using System.Threading;

namespace Ncp.CleanDDD.Web.Application.Queries
{
    public class OrderQuery(ApplicationDbContext applicationDbContext)
    {
        public async Task<Order?> QueryOrder(OrderId orderId, CancellationToken cancellationToken)
        {
            return await applicationDbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken);
        }
    }
}
