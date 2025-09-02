using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate
{
    public class UserRole
    {
        protected UserRole() { }

        public UserId UserId { get; private set; } = default!;
        public RoleId RoleId { get; private set; } = default!;
        public string RoleName { get; private set; } = string.Empty;

        public UserRole(RoleId roleId, string roleName)
        {
            RoleId = roleId;
            RoleName = roleName;
        }

        public void UpdateRoleInfo(string roleName)
        {
            RoleName = roleName;
        }
    }
}
