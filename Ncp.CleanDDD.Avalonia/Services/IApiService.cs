using System.Collections.Generic;
using System.Threading.Tasks;
using Ncp.CleanDDD.Avalonia.Models;

namespace Ncp.CleanDDD.Avalonia.Services
{
    /// <summary>
    /// API服务接口
    /// </summary>
    public interface IApiService
    {
        // 认证相关
        void SetAuthToken(string token);
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginCredentials credentials);
        Task<ApiResponse<bool>> LogoutAsync();

        // 用户管理
        Task<ApiResponse<PagedData<User>>> GetUsersAsync(UserQueryInput query);
        Task<ApiResponse<User>> GetUserByIdAsync(string userId);
        Task<ApiResponse<bool>> CreateUserAsync(User user);
        Task<ApiResponse<bool>> UpdateUserAsync(User user);
        Task<ApiResponse<bool>> DeleteUserAsync(string userId);
        Task<ApiResponse<bool>> ResetPasswordAsync(string userId);

        // 角色管理
        Task<ApiResponse<PagedData<Role>>> GetRolesAsync(RoleQueryInput query);
        Task<ApiResponse<Role>> GetRoleByIdAsync(string roleId);
        Task<ApiResponse<bool>> CreateRoleAsync(Role role);
        Task<ApiResponse<bool>> UpdateRoleAsync(Role role);
        Task<ApiResponse<bool>> DeleteRoleAsync(string roleId);

        // 组织架构管理
        Task<ApiResponse<List<OrganizationUnit>>> GetOrganizationUnitsAsync(OrganizationUnitQueryInput query);
        Task<ApiResponse<List<OrganizationUnitTree>>> GetOrganizationUnitTreeAsync(bool includeInactive = false);
        Task<ApiResponse<OrganizationUnit>> GetOrganizationUnitByIdAsync(int id);
        Task<ApiResponse<bool>> CreateOrganizationUnitAsync(CreateOrganizationUnitRequest request);
        Task<ApiResponse<bool>> UpdateOrganizationUnitAsync(UpdateOrganizationUnitRequest request);
        Task<ApiResponse<bool>> DeleteOrganizationUnitAsync(int id);

        // 权限管理
        Task<ApiResponse<List<PermissionGroup>>> GetPermissionTreeAsync();
    }
}
