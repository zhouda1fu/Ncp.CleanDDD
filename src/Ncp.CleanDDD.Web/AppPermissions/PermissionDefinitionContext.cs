﻿using System.Collections.Immutable;
using System.Security;

namespace Ncp.CleanDDD.Web.AppPermissions
{
    /// <summary>
    /// 管理权限定义的上下文类，负责初始化和提供权限组及其权限项。
    /// </summary>
    public static class PermissionDefinitionContext
    {
        // 存储权限组的字典，键为权限组名称，值为权限组对象
        private static Dictionary<string, AppPermissionGroup> Groups { get; } = new();

        // 静态构造函数，在类初始化时创建默认的权限组和权限项
        static PermissionDefinitionContext()
        {
            var systemAccess = AddGroup("SystemAccess");
            var adminUserManagement = systemAccess.AddPermission(PermissionCodes.UserManagement, "用户管理");
            adminUserManagement.AddChild(PermissionCodes.UserCreate, "创建用户");
            adminUserManagement.AddChild(PermissionCodes.UserEdit, "编辑用户");
            adminUserManagement.AddChild(PermissionCodes.UserDelete, "删除用户");
            adminUserManagement.AddChild(PermissionCodes.UserView, "查看用户");
            adminUserManagement.AddChild(PermissionCodes.UserRoleAssign, "分配用户角色");
            adminUserManagement.AddChild(PermissionCodes.UserResetPassword, "重置用户密码");
            var roleManagement = systemAccess.AddPermission(PermissionCodes.RoleManagement, "角色管理");
            roleManagement.AddChild(PermissionCodes.RoleCreate, "创建角色");
            roleManagement.AddChild(PermissionCodes.RoleEdit, "编辑角色");
            roleManagement.AddChild(PermissionCodes.RoleDelete, "删除角色");
            roleManagement.AddChild(PermissionCodes.RoleView, "查看角色");
            roleManagement.AddChild(PermissionCodes.RoleUpdatePermissions, "更新角色权限");

            //var systemAdmin = systemAccess.AddPermission(PermissionCodes.SystemAdmin, "系统管理员权限");
            var systemMonitor = systemAccess.AddPermission(PermissionCodes.SystemMonitor, "系统监控");
            systemMonitor.AddChild(PermissionCodes.LogView, "查看系统日志");

            // 组织架构管理权限
            var organizationUnitManagement = systemAccess.AddPermission(PermissionCodes.OrganizationUnitManagement, "组织架构管理");
            organizationUnitManagement.AddChild(PermissionCodes.OrganizationUnitCreate, "创建组织架构");
            organizationUnitManagement.AddChild(PermissionCodes.OrganizationUnitEdit, "编辑组织架构");
            organizationUnitManagement.AddChild(PermissionCodes.OrganizationUnitDelete, "删除组织架构");
            organizationUnitManagement.AddChild(PermissionCodes.OrganizationUnitView, "查看组织架构");
            organizationUnitManagement.AddChild(PermissionCodes.OrganizationUnitAssign, "分配组织架构");

            // 所有接口访问权限
            var allApiAccess = systemAccess.AddPermission(PermissionCodes.AllApiAccess, "所有接口访问权限");
        }

        /// <summary>
        /// 添加一个新的权限组，如果权限组名称已存在则抛出异常。
        /// </summary>
        /// <param name="name">权限组名称</param>
        /// <returns>返回创建的权限组</returns>
        /// <exception cref="ArgumentException">如果权限组名称已经存在，则抛出异常</exception>
        private static AppPermissionGroup AddGroup(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (Groups.ContainsKey(name))
            {
                throw new ArgumentException($"There is already an existing permission group with name: {name}");
            }

            return Groups[name] = new AppPermissionGroup(name);
        }

        /// <summary>
        /// 获取所有的权限组。
        /// </summary>
        public static IReadOnlyList<AppPermissionGroup> PermissionGroups => Groups.Values.ToImmutableList();
    }
}
