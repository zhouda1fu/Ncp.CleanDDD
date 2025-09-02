namespace Ncp.CleanDDD.Web.AppPermissions
{
    /// <summary>
    /// 权限常量定义
    /// </summary>
    public static class PermissionCodes
    {
        #region 角色管理权限
        public const string RoleManagement = nameof(RoleManagement);
        public const string RoleCreate = nameof(RoleCreate);
        public const string RoleEdit = nameof(RoleEdit);
        public const string RoleDelete = nameof(RoleDelete);
        public const string RoleView = nameof(RoleView);
        public const string RoleUpdatePermissions = nameof(RoleUpdatePermissions);
        #endregion

        #region 用户管理权限
        public const string UserManagement = nameof(UserManagement);
        public const string UserCreate = nameof(UserCreate);
        public const string UserEdit = nameof(UserEdit);
        public const string UserDelete = nameof(UserDelete);
        public const string UserView = nameof(UserView);
        public const string UserRoleAssign = nameof(UserRoleAssign);
        public const string UserResetPassword = nameof(UserResetPassword);
        #endregion

        #region 系统管理权限
        public const string SystemAdmin = nameof(SystemAdmin);
        public const string SystemMonitor = nameof(SystemMonitor);
        #endregion

        #region 组织架构管理权限
        public const string OrganizationUnitManagement = nameof(OrganizationUnitManagement);
        public const string OrganizationUnitCreate = nameof(OrganizationUnitCreate);
        public const string OrganizationUnitEdit = nameof(OrganizationUnitEdit);
        public const string OrganizationUnitDelete = nameof(OrganizationUnitDelete);
        public const string OrganizationUnitView = nameof(OrganizationUnitView);
        public const string OrganizationUnitAssign = nameof(OrganizationUnitAssign);
        #endregion

        #region 设备状态管理权限
        public const string DeviceManagement = nameof(DeviceManagement);
        public const string DeviceCreate = nameof(DeviceCreate);
        public const string DeviceEdit = nameof(DeviceEdit);
        public const string DeviceDelete = nameof(DeviceDelete);
        public const string DeviceView = nameof(DeviceView);
        #endregion

        #region 团体任务管理权限
        public const string GroupTaskManagement = nameof(GroupTaskManagement);
        public const string GroupTaskCreate = nameof(GroupTaskCreate);
        public const string GroupTaskEdit = nameof(GroupTaskEdit);
        public const string GroupTaskDelete = nameof(GroupTaskDelete);
        public const string GroupTaskView = nameof(GroupTaskView);
        #endregion



        #region 在线团体任务管理权限
        //public const string GroupTaskListManagement = nameof(GroupTaskListManagement);
        //public const string GroupTaskListCreate = nameof(GroupTaskListCreate);
        //public const string GroupTaskListEdit = nameof(GroupTaskListEdit);
        //public const string GroupTaskListDelete = nameof(GroupTaskListDelete);
        //public const string GroupTaskListView = nameof(GroupTaskListView);
        #endregion


        #region 任务课程管理权限
        public const string CourseManagement = nameof(CourseManagement);
        public const string CourseCreate = nameof(CourseCreate);
        public const string CourseEdit = nameof(CourseEdit);
        public const string CourseDelete = nameof(CourseDelete);
        public const string CourseView = nameof(CourseView);
        #endregion

        #region 用户训练档案管理权限
        public const string UserTrainingArchiveManagement = nameof(UserTrainingArchiveManagement);
        public const string UserTrainingArchiveView = nameof(UserTrainingArchiveView);
        public const string UserTrainingArchiveDelete = nameof(UserTrainingArchiveDelete);
        #endregion

        #region 日志查看权限
        public const string LogView = nameof(LogView);
        #endregion

        #region 所有接口访问权限
        public const string AllApiAccess = nameof(AllApiAccess);

        #endregion
    }
} 