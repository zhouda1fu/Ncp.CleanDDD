using System;
using System.Collections.Generic;

namespace Ncp.CleanDDD.Avalonia.Models
{
    /// <summary>
    /// 组织架构模型
    /// </summary>
    public class OrganizationUnit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    /// <summary>
    /// 组织架构树形模型
    /// </summary>
    public class OrganizationUnitTree : OrganizationUnit
    {
        public List<OrganizationUnitTree> Children { get; set; } = new();
    }

    /// <summary>
    /// 组织架构查询输入参数
    /// </summary>
    public class OrganizationUnitQueryInput
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public int? ParentId { get; set; }
    }

    /// <summary>
    /// 创建组织架构请求
    /// </summary>
    public class CreateOrganizationUnitRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int SortOrder { get; set; } = 1;
    }

    /// <summary>
    /// 更新组织架构请求
    /// </summary>
    public class UpdateOrganizationUnitRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
    }
}
