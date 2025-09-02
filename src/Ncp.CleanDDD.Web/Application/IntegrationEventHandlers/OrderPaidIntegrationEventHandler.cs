﻿using Ncp.CleanDDD.Web.Application.Commands;
using DotNetCore.CAP;
using MediatR;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.Primitives;

namespace Ncp.CleanDDD.Web.Application.IntegrationEventHandlers
{
    public class OrderPaidIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<OrderPaidIntegrationEvent>
    {
        public Task HandleAsync(OrderPaidIntegrationEvent eventData, CancellationToken cancellationToken = default)
        {
            var cmd = new OrderPaidCommand(eventData.OrderId);
            return mediator.Send(cmd, cancellationToken);
        }
    }
}