using Ncp.CleanDDD.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.CleanDDD.Domain.DomainEvents
{
    public record OrderCreatedDomainEvent(Order Order) : IDomainEvent;
}
