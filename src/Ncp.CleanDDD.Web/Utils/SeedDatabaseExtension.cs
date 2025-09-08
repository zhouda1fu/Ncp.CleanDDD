using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Utils
{
    public static class SeedDatabaseExtension
    {
        internal static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 初始化角色和权限
            if (!dbContext.Roles.Any())
            {
                var adminPermissions = new List<RolePermission>
            {
                new RolePermission(PermissionCodes.UserCreate, "创建用户", "创建新用户"),
                new RolePermission(PermissionCodes.UserView, "查看用户", "查看用户信息"),
                new RolePermission(PermissionCodes.UserEdit, "更新用户", "更新用户信息"),
                new RolePermission(PermissionCodes.UserDelete, "删除用户", "删除用户"),
                new RolePermission(PermissionCodes.RoleCreate, "创建角色", "创建新角色"),
                new RolePermission(PermissionCodes.RoleView, "查看角色", "查看角色信息"),
                new RolePermission(PermissionCodes.RoleEdit, "更新角色", "更新角色信息"),
                new RolePermission(PermissionCodes.RoleDelete, "删除角色", "删除角色"),
                new RolePermission(PermissionCodes.UserRoleAssign, "分配用户角色", "分配用户角色权限"),
                new RolePermission(PermissionCodes.UserResetPassword, "重置用户密码", "重置用户密码"),
                new RolePermission(PermissionCodes.RoleUpdatePermissions, "更新角色权限", "更新角色的权限"),
                //new RolePermission(PermissionCodes.SystemAdmin, "系统管理员权限", "拥有系统管理员权限"),
                new RolePermission(PermissionCodes.SystemMonitor, "系统监控权限", "拥有系统监控权限"),
                new RolePermission(PermissionCodes.OrganizationUnitCreate, "创建组织机构", "创建组织机构"),
                new RolePermission(PermissionCodes.OrganizationUnitView, "查看组织机构", "查看组织机构信息"),
                new RolePermission(PermissionCodes.OrganizationUnitEdit, "更新组织机构", "更新组织机构信息"),
                new RolePermission(PermissionCodes.OrganizationUnitDelete, "删除组织机构", "删除组织机构"),
                new RolePermission(PermissionCodes.OrganizationUnitAssign, "分配组织架构", "分配组织架构"),
              

                new RolePermission(PermissionCodes.AllApiAccess, "所有接口访问权限", "所有接口访问权限"),

            };

                var userPermissions = new List<RolePermission>
            {
                new RolePermission(PermissionCodes.UserView, "查看用户", "查看用户信息"),
                new RolePermission(PermissionCodes.UserEdit, "更新用户", "更新自己的用户信息"),
                 new RolePermission(PermissionCodes.AllApiAccess, "所有接口访问权限", "所有接口访问权限"),
            };

                var adminRole = new Role("管理员", "系统管理员", adminPermissions);
                var userRole = new Role("普通用户", "普通用户", userPermissions);

                dbContext.Roles.Add(adminRole);
                dbContext.Roles.Add(userRole);
                dbContext.SaveChanges();
            }



            // 初始化组织机构
            if (!dbContext.OrganizationUnits.Any())
            {
                var organizationUnit = new OrganizationUnit("大组", "根节点", new OrganizationUnitId(0), 1);

                dbContext.OrganizationUnits.Add(organizationUnit);
                dbContext.SaveChanges();

                var organizationUnitId = dbContext.OrganizationUnits.First(r => r.Name == "大组").Id;
                var childGroup = new OrganizationUnit("小组", "第一个子节点", organizationUnitId, 1);
                dbContext.OrganizationUnits.Add(childGroup);
                dbContext.SaveChanges();

                var childGroupId = dbContext.OrganizationUnits.First(r => r.Name == "小组").Id;
                var childIndividual = new OrganizationUnit("个人", "第一个子节点的子节点", childGroupId, 1);
                dbContext.OrganizationUnits.Add(childIndividual);
                dbContext.SaveChanges();

            }


            // 初始化管理员用户
            if (!dbContext.Users.Any(u => u.Name == "admin"))
            {

                var organizationUnit = dbContext.OrganizationUnits.First(r => r.Name == "大组");
                var adminRole = dbContext.Roles.First(r => r.Name == "管理员");
                var adminUser = new User(
                    "admin",
                    "13800138000",
                    PasswordHasher.HashPassword("123456"),
                    new List<UserRole> { new UserRole(adminRole.Id, adminRole.Name) },
                    "系统管理员",
                    1,
                    "admin@example.com",
                    "男",
                    DateTimeOffset.Now.AddYears(-30) // 假设管理员年龄为30岁
                );

                // 设置组织架构关系
                adminUser.AssignOrganizationUnit(new UserOrganizationUnit(adminUser.Id, organizationUnit.Id, organizationUnit.Name));
                dbContext.Users.Add(adminUser);
                dbContext.SaveChanges();
            }


            // 初始化测试用户
            if (!dbContext.Users.Any(u => u.Name == "test"))
            {
                var organizationUnit = dbContext.OrganizationUnits.First(r => r.Name == "大组");
                var userRole = dbContext.Roles.First(r => r.Name == "普通用户");
                var testUser = new User(
                    "test",
                    "13800138001",
                    PasswordHasher.HashPassword("123456"),
                    new List<UserRole> { new UserRole(userRole.Id, userRole.Name) },
                    "测试用户",
                    1,
                    "test@example.com",
                    "女",
                    DateTimeOffset.Now.AddYears(-30) // 假设管理员年龄为30岁
                );

                testUser.AssignOrganizationUnit(new UserOrganizationUnit(testUser.Id, organizationUnit.Id, organizationUnit.Name));
                dbContext.Users.Add(testUser);
                dbContext.SaveChanges();
            }


           



            return app;
        }
    }
}
