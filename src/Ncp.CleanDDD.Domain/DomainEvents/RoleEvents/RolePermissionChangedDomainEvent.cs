using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Domain.DomainEvents.RoleEvents
{
    public record RolePermissionChangedDomainEvent(Role Role) : IDomainEvent;
}
