using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate
{
    public class UserOrganizationUnit
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public UserId UserId { get; private set; } = default!;

        /// <summary>
        /// 组织架构ID
        /// </summary>
        public OrganizationUnitId OrganizationUnitId { get; private set; } = default!;

        public string OrganizationUnitName { get; private set; } = string.Empty;


        /// <summary>
        /// 分配时间
        /// </summary>
        public DateTime AssignedAt { get; init; }



        public UserOrganizationUnit(UserId userId, OrganizationUnitId organizationUnitId, string organizationUnitName)
        {
            UserId = userId;
            OrganizationUnitId = organizationUnitId;
            AssignedAt = DateTime.UtcNow;
            OrganizationUnitName = organizationUnitName;
        }


    }
}
