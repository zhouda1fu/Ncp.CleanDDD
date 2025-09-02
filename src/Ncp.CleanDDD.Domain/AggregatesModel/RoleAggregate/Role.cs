using Ncp.CleanDDD.Domain.DomainEvents.RoleEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate
{
    public partial record RoleId : IGuidStronglyTypedId;


    public class Role : Entity<RoleId>, IAggregateRoot
    {
        protected Role()
        {
        }

        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// 删除标记
        /// </summary>
        public Deleted IsDeleted { get; private set; } = new Deleted(false);

        /// <summary>
        /// 删除时间
        /// </summary>
        public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTime.UtcNow);

        public virtual ICollection<RolePermission> Permissions { get; init; } = [];




        public Role(string name, string description, IEnumerable<RolePermission> permissions)
        {
            CreatedAt = DateTime.Now;
            Name = name;
            Description = description;
            Permissions = new List<RolePermission>(permissions);
            IsActive = true;
        }

        public void UpdateRoleInfo(string name, string description)
        {
            Name = name;
            Description = description;
            AddDomainEvent(new RoleInfoChangedDomainEvent(this));
        }

        public void UpdateRolePermissions(IEnumerable<RolePermission> newPermissions)
        {
            var currentPermissionMap = Permissions.ToDictionary(p => p.PermissionCode);
            var newPermissionMap = newPermissions.ToDictionary(p => p.PermissionCode);

            var permissionsToRemove = currentPermissionMap.Keys.Except(newPermissionMap.Keys).ToList();
            foreach (var permissionCode in permissionsToRemove)
            {
                Permissions.Remove(currentPermissionMap[permissionCode]);
            }

            var permissionsToAdd = newPermissionMap.Keys.Except(currentPermissionMap.Keys).ToList();
            foreach (var permissionCode in permissionsToAdd)
            {
                Permissions.Add(newPermissionMap[permissionCode]);
            }

            AddDomainEvent(new RolePermissionChangedDomainEvent(this));
        }

        public void Deactivate()
        {
            if (!IsActive)
            {
                throw new KnownException("角色已经被停用");
            }

            IsActive = false;
        }

        public void Activate()
        {
            if (IsActive)
            {
                throw new KnownException("角色已经是激活状态");
            }

            IsActive = true;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
        }

        public bool HasPermission(string permissionCode)
        {
            return Permissions.Any(p => p.PermissionCode == permissionCode);
        }
    }
}
