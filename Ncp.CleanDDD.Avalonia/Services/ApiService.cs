using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ncp.CleanDDD.Avalonia.Models;

namespace Ncp.CleanDDD.Avalonia.Services
{
    /// <summary>
    /// API服务实现
    /// </summary>
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        /// <summary>
        /// 设置认证token
        /// </summary>
        /// <param name="token">JWT token</param>
        public void SetAuthToken(string token)
        {
            if (_tokenProvider is TokenProvider tokenProvider)
            {
                tokenProvider.SetToken(token);
            }
        }

        #region 用户管理

        public async Task<ApiResponse<PagedData<User>>> GetUsersAsync(UserQueryInput query)
        {
            try
            {
                var queryString = BuildQueryString(query);
                var response = await _httpClient.GetAsync($"/api/users{queryString}");
                return await HandleResponse<ApiResponse<PagedData<User>>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表失败");
                return CreateErrorResponse<PagedData<User>>("获取用户列表失败");
            }
        }

        public async Task<ApiResponse<User>> GetUserByIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}");
                return await HandleResponse<ApiResponse<User>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败");
                return CreateErrorResponse<User>("获取用户信息失败");
            }
        }

        public async Task<ApiResponse<bool>> CreateUserAsync(User user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/users", content);
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户失败");
                return CreateErrorResponse<bool>("创建用户失败");
            }
        }

        public async Task<ApiResponse<bool>> UpdateUserAsync(User user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/api/users/{user.UserId}", content);
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户失败");
                return CreateErrorResponse<bool>("更新用户失败");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/users/{userId}");
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败");
                return CreateErrorResponse<bool>("删除用户失败");
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/users/{userId}/reset-password", null);
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置密码失败");
                return CreateErrorResponse<bool>("重置密码失败");
            }
        }

        #endregion

        #region 角色管理

        public async Task<ApiResponse<PagedData<Role>>> GetRolesAsync(RoleQueryInput query)
        {
            try
            {
                var queryString = BuildQueryString(query);
                var response = await _httpClient.GetAsync($"/api/roles{queryString}");
                return await HandleResponse<ApiResponse<PagedData<Role>>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取角色列表失败");
                return CreateErrorResponse<PagedData<Role>>("获取角色列表失败");
            }
        }

        public async Task<ApiResponse<Role>> GetRoleByIdAsync(string roleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/roles/{roleId}");
                return await HandleResponse<ApiResponse<Role>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取角色信息失败");
                return CreateErrorResponse<Role>("获取角色信息失败");
            }
        }

        public async Task<ApiResponse<bool>> CreateRoleAsync(Role role)
        {
            try
            {
                var json = JsonSerializer.Serialize(role, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/roles", content);
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建角色失败");
                return CreateErrorResponse<bool>("创建角色失败");
            }
        }

        public async Task<ApiResponse<bool>> UpdateRoleAsync(Role role)
        {
            try
            {
                var json = JsonSerializer.Serialize(role, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/api/roles/{role.RoleId}", content);
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新角色失败");
                return CreateErrorResponse<bool>("更新角色失败");
            }
        }

        public async Task<ApiResponse<bool>> DeleteRoleAsync(string roleId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/roles/{roleId}");
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除角色失败");
                return CreateErrorResponse<bool>("删除角色失败");
            }
        }

        #endregion

        #region 组织架构管理

        public async Task<ApiResponse<List<OrganizationUnit>>> GetOrganizationUnitsAsync(OrganizationUnitQueryInput query)
        {
            try
            {
                var queryString = BuildQueryString(query);
                var response = await _httpClient.GetAsync($"/api/organization-units{queryString}");
                return await HandleResponse<ApiResponse<List<OrganizationUnit>>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取组织架构列表失败");
                return CreateErrorResponse<List<OrganizationUnit>>("获取组织架构列表失败");
            }
        }

        public async Task<ApiResponse<List<OrganizationUnitTree>>> GetOrganizationUnitTreeAsync(bool includeInactive = false)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/organization-units/tree?includeInactive={includeInactive}");
                return await HandleResponse<ApiResponse<List<OrganizationUnitTree>>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取组织架构树失败");
                return CreateErrorResponse<List<OrganizationUnitTree>>("获取组织架构树失败");
            }
        }

        public async Task<ApiResponse<OrganizationUnit>> GetOrganizationUnitByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/organization-units/{id}");
                return await HandleResponse<ApiResponse<OrganizationUnit>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取组织架构信息失败");
                return CreateErrorResponse<OrganizationUnit>("获取组织架构信息失败");
            }
        }

        public async Task<ApiResponse<bool>> CreateOrganizationUnitAsync(CreateOrganizationUnitRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/organization-units", content);
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建组织架构失败");
                return CreateErrorResponse<bool>("创建组织架构失败");
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrganizationUnitAsync(UpdateOrganizationUnitRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/api/organization-units/{request.Id}", content);
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新组织架构失败");
                return CreateErrorResponse<bool>("更新组织架构失败");
            }
        }

        public async Task<ApiResponse<bool>> DeleteOrganizationUnitAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/organization-units/{id}");
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除组织架构失败");
                return CreateErrorResponse<bool>("删除组织架构失败");
            }
        }

        #endregion

        #region 权限管理

        public async Task<ApiResponse<List<PermissionGroup>>> GetPermissionTreeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/permissions/tree");
                return await HandleResponse<ApiResponse<List<PermissionGroup>>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取权限树失败");
                return CreateErrorResponse<List<PermissionGroup>>("获取权限树失败");
            }
        }

        #endregion

        #region 认证

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginCredentials credentials)
        {
            try
            {
                var json = JsonSerializer.Serialize(credentials, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                // 后端实际路由为 /api/user/login （单数）
                var response = await _httpClient.PostAsync("/api/user/login", content);
                return await HandleResponse<ApiResponse<LoginResponse>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败");
                return CreateErrorResponse<LoginResponse>("登录失败");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/users/logout", null);
                return await HandleResponse<ApiResponse<bool>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登出失败");
                return CreateErrorResponse<bool>("登出失败");
            }
        }

        #endregion

        #region 私有方法

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(content, _jsonOptions) ?? throw new InvalidOperationException("反序列化响应失败");
            }
            else
            {
                throw new HttpRequestException($"HTTP错误: {response.StatusCode}, 内容: {content}");
            }
        }

        private ApiResponse<T> CreateErrorResponse<T>(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Code = -1,
                Data = default(T)
            };
        }

        private string BuildQueryString(object query)
        {
            var properties = query.GetType().GetProperties();
            var queryParams = new List<string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(query);
                if (value != null)
                {
                    queryParams.Add($"{prop.Name}={Uri.EscapeDataString(value.ToString() ?? string.Empty)}");
                }
            }

            return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
        }

        #endregion
    }
}
