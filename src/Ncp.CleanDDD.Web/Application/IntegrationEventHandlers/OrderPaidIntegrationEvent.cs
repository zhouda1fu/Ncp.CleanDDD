using Ncp.CleanDDD.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.CleanDDD.Web.Application.IntegrationEventHandlers
{
    public record OrderPaidIntegrationEvent(OrderId OrderId);
}
