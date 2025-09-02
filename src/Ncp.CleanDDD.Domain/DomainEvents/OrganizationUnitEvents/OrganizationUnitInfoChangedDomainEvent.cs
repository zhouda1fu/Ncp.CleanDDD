using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Domain.DomainEvents.OrganizationUnitEvents
{
    public record OrganizationUnitInfoChangedDomainEvent(OrganizationUnit OrganizationUnit) : IDomainEvent
    {
    }
}
