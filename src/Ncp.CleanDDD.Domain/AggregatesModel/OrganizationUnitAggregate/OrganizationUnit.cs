using Ncp.CleanDDD.Domain.DomainEvents.OrganizationUnitEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate
{
    public partial record OrganizationUnitId : IInt64StronglyTypedId;

    public class OrganizationUnit : Entity<OrganizationUnitId>, IAggregateRoot
    {

        /// <summary>
        /// 组织架构名称
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// 上级组织架构ID
        /// </summary>
        public OrganizationUnitId ParentId { get; private set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; private set; } = false;

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTimeOffset? DeletedAt { get; private set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

        /// <summary>
        /// 子组织架构
        /// </summary>
        [NotMapped]
        public virtual ICollection<OrganizationUnit> Children { get; } = [];



        public OrganizationUnit(string name, string description, OrganizationUnitId parentId, int sortOrder)
        {
            CreatedAt = DateTimeOffset.Now;
            Name = name;
            Description = description;
            ParentId = parentId;
            SortOrder = sortOrder;
            IsActive = true;
        }

        /// <summary>
        /// 更新组织架构信息
        /// </summary>
        public void UpdateInfo(string name, string description, OrganizationUnitId parentId, int sortOrder)
        {
            Name = name;
            Description = description;
            ParentId = parentId;
            SortOrder = sortOrder;
            UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);

            AddDomainEvent(new OrganizationUnitInfoChangedDomainEvent(this));
        }

        /// <summary>
        /// 激活组织架构
        /// </summary>
        public void Activate()
        {
            if (IsActive)
            {
                throw new KnownException("组织架构已经是激活状态");
            }

            IsActive = true;
            UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 停用组织架构
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
            {
                throw new KnownException("组织架构已经被停用");
            }

            IsActive = false;
            UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 删除组织架构
        /// </summary>
        public void Delete()
        {
            if (IsDeleted)
            {
                throw new KnownException("组织架构已经被删除");
            }

            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
            UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 添加子组织架构
        /// </summary>
        public void AddChild(OrganizationUnit child)
        {
            if (child == null)
            {
                throw new KnownException("子组织架构不能为空");
            }

            Children.Add(child);
        }

        /// <summary>
        /// 移除子组织架构
        /// </summary>
        public void RemoveChild(OrganizationUnit child)
        {
            if (child == null)
            {
                throw new KnownException("子组织架构不能为空");
            }

            Children.Remove(child);
        }

        /// <summary>
        /// 获取所有子组织架构（包括子级的子级）
        /// </summary>
        public IEnumerable<OrganizationUnit> GetAllChildren()
        {
            var result = new List<OrganizationUnit>();
            foreach (var child in Children)
            {
                result.Add(child);
                result.AddRange(child.GetAllChildren());
            }
            return result;
        }

        /// <summary>
        /// 获取组织架构层级路径
        /// </summary>
        public string GetPath()
        {
            return Name;
        }
    }
}
