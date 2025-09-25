using System;
using System.Collections.Generic;

namespace Ncp.CleanDDD.Avalonia.Models
{
    /// <summary>
    /// 角色模型
    /// </summary>
    public class Role
    {
        public string RoleId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> PermissionCodes { get; set; } = new();
    }

    /// <summary>
    /// 角色查询输入参数
    /// </summary>
    public class RoleQueryInput
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool CountTotal { get; set; } = true;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// 权限模型
    /// </summary>
    public class Permission
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public List<Permission> Children { get; set; } = new();
    }

    /// <summary>
    /// 权限组模型
    /// </summary>
    public class PermissionGroup
    {
        public string Name { get; set; } = string.Empty;
        public List<Permission> Permissions { get; set; } = new();
    }
}
