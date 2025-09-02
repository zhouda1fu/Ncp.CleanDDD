using Ncp.CleanDDD.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.CleanDDD.Domain.DomainEvents;

public record OrderPaidDomainEvent(Order Order) : IDomainEvent;