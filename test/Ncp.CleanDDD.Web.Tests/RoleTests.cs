using Microsoft.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Infrastructure;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;
using Ncp.CleanDDD.Web.Endpoints.UserEndpoints;
using Ncp.CleanDDD.Web.Tests.Extensions;
using NetCorePal.Extensions.Dto;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Ncp.CleanDDD.Web.Tests;

[Collection("web")]
public class RoleTests : IDisposable
{
    private readonly HttpClient _client;
    private string? _authToken;
    private string? _refreshToken;
    private readonly List<RoleId> _createdRoleIds = new();

    // 测试数据常量
    private const string TestRoleName = "测试角色";
    private const string TestRoleDescription = "这是一个测试角色";
    private readonly string[] TestPermissionCodes = { "test.read", "test.write" };

    public RoleTests(MyWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder => { builder.ConfigureServices(_ => { }); })
            .CreateClient();

        // 在构造函数中登录获取token
        LoginAndGetToken().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        // 清理测试过程中创建的角色
        CleanupTestRoles().GetAwaiter().GetResult();
    }

    private async Task CleanupTestRoles()
    {
        if (_authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            
            foreach (var roleId in _createdRoleIds)
            {
                try
                {
                    await _client.DeleteAsync($"/api/roles/{roleId}");
                }
                catch
                {
                    // 忽略清理过程中的错误
                }
            }
        }
    }

    private async Task LoginAndGetToken()
    {
        const string json = $$"""
                              {
                                   "username": "{{AppDefaultCredentials.UserName}}",
                                   "password": "{{AppDefaultCredentials.Password}}"
                              }
                              """;
        var content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await _client.PostAsync("api/user/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<LoginResponse>>();
            if (responseData?.Data != null)
            {
                _authToken = responseData.Data.Token;
                _refreshToken = responseData.Data.RefreshToken;

                // 设置认证头
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            }
        }
    }

    private void SetAuthHeader(bool useAuth = true)
    {
        if (useAuth && _authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }
        else
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }
    }

    private CreateRoleRequest CreateTestRoleRequest(string? name = null, string? description = null, string[]? permissionCodes = null)
    {
        return new CreateRoleRequest(
            Name: name ?? $"{TestRoleName}_{Guid.NewGuid():N}",
            Description: description ?? TestRoleDescription,
            PermissionCodes: permissionCodes ?? TestPermissionCodes
        );
    }

    [Fact]
    public async Task CreateRole_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var uniqueName = $"创建角色测试_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(uniqueName, "创建角色测试描述");

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", createRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateRoleResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal(uniqueName, responseData.Data.Name);
        Assert.Equal("创建角色测试描述", responseData.Data.Description);

        // 记录创建的角色ID用于清理
        if (Guid.TryParse(responseData.Data.RoleId, out var roleGuid))
        {
            _createdRoleIds.Add(new RoleId(roleGuid));
        }
    }

    [Fact]
    public async Task CreateRole_DuplicateName_ShouldFail()
    {
        // Arrange
        var roleName = $"重复角色名_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(roleName);

        // 先创建一个角色
        var firstResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        Assert.True(firstResponse.IsSuccessStatusCode);
        var firstResponseData = await firstResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateRoleResponse>>();
        if (Guid.TryParse(firstResponseData!.Data!.RoleId, out var roleGuid))
        {
            _createdRoleIds.Add(new RoleId(roleGuid));
        }

        // Act - 尝试创建相同名称的角色
        var duplicateRequest = CreateTestRoleRequest(roleName);
        var response = await _client.PostAsJsonAsync("/api/roles", duplicateRequest);
        var duplicateResponseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateRoleResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(duplicateResponseData);
        Assert.Equal(400, duplicateResponseData.Code);
        Assert.Contains("该角色已存在", duplicateResponseData.Message); 
    }

    [Fact]
    public async Task GetAllRoles_ShouldSucceed()
    {
        // Arrange
        var queryInput = new RoleQueryInput
        {
            PageIndex = 1,
            PageSize = 10,
            Name = null,
            Description = null,
            IsActive = null,
            CountTotal = true
        };

        // Act
        var queryString = $"?pageIndex={queryInput.PageIndex}&pageSize={queryInput.PageSize}&countTotal={queryInput.CountTotal}";
        var response = await _client.GetAsync($"/api/roles{queryString}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<PagedData<RoleQueryDto>>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.NotNull(responseData.Data.Items);
    }

    [Fact]
    public async Task GetAllRoles_WithNameFilter_ShouldSucceed()
    {
        // Arrange
        var testRoleName = $"筛选测试角色_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(testRoleName);
        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateRoleResponse>>();
        if (Guid.TryParse(createResponseData!.Data!.RoleId, out var roleGuid))
        {
            _createdRoleIds.Add(new RoleId(roleGuid));
        }

        // Act
        var queryString = $"?pageIndex=1&pageSize=10&name={testRoleName}&countTotal=true";
        var response = await _client.GetAsync($"/api/roles{queryString}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<PagedData<RoleQueryDto>>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.NotNull(responseData.Data.Items);
        Assert.True(responseData.Data.Items.Any(r => r.Name == testRoleName));
    }

    [Fact]
    public async Task GetRole_ValidId_ShouldSucceed()
    {
        // Arrange - 先创建一个角色
        var testRoleName = $"获取角色测试_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(testRoleName);
        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateRoleResponse>>();
        Assert.NotNull(createResponseData);
        
        var roleId = createResponseData.Data!.RoleId;
        if (Guid.TryParse(roleId, out var roleGuid))
        {
            _createdRoleIds.Add(new RoleId(roleGuid));
        }

        // Act
        var response = await _client.GetAsync($"/api/roles/{roleId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<GetRoleResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal(testRoleName, responseData.Data.Name);
    }

    [Fact]
    public async Task GetRole_InvalidId_ShouldFail()
    {
        // Arrange
        var invalidRoleId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/roles/{invalidRoleId}");
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<GetRoleResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(responseData);
        Assert.Contains("Invalid Credentials", responseData.Message);
    }

    [Fact]
    public async Task UpdateRole_ValidRequest_ShouldSucceed()
    {
        // Arrange - 先创建一个角色
        var originalName = $"更新前角色_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(originalName);
        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateRoleResponse>>();
        Assert.NotNull(createResponseData);
        
        var roleId = createResponseData.Data!.RoleId;
        if (Guid.TryParse(roleId, out var roleGuid))
        {
            _createdRoleIds.Add(new RoleId(roleGuid));
        }

        var updatedName = $"更新后角色_{Guid.NewGuid():N}";
        var updateRequest = new UpdateRoleInfoRequest(
            RoleId: new RoleId(roleGuid),
            Name: updatedName,
            Description: "更新后的描述",
            PermissionCodes: new[] { "updated.read", "updated.write" }
        );

        // Act
        var response = await _client.PutAsNewtonsoftJsonAsync("/api/roles/update", updateRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(responseData);
        Assert.True(responseData.Data);

        // 验证更新是否成功
        var getResponse = await _client.GetAsync($"/api/roles/{roleId}");
        var getRoleData = await getResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<GetRoleResponse>>();
        Assert.NotNull(getRoleData);
        Assert.Equal(updatedName, getRoleData.Data!.Name);
        Assert.Equal("更新后的描述", getRoleData.Data.Description);
    }

    [Fact]
    public async Task UpdateRole_InvalidId_ShouldFail()
    {
        // Arrange
        var invalidRoleId = new RoleId(Guid.NewGuid());
        var updateRequest = new UpdateRoleInfoRequest(
            RoleId: invalidRoleId,
            Name: "无效ID更新测试",
            Description: "这应该会失败",
            PermissionCodes: new[] { "test.read" }
        );

        // Act
        var response = await _client.PutAsNewtonsoftJsonAsync("/api/roles/update", updateRequest);

        // Assert
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(responseData);
        Assert.Contains("未找到角色", responseData.Message);
    }

    [Fact]
    public async Task DeleteRole_ValidId_ShouldSucceed()
    {
        // Arrange - 先创建一个角色
        var testRoleName = $"待删除角色_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(testRoleName);
        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateRoleResponse>>();
        Assert.NotNull(createResponseData);
        
        var roleId = createResponseData.Data!.RoleId;

        // Act
        var response = await _client.DeleteAsync($"/api/roles/{roleId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(responseData);
        Assert.True(responseData.Data);

        // 验证角色确实被删除了
        var getResponse = await _client.GetAsync($"/api/roles/{roleId}");
        var getResponseData = await getResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();

        Assert.True(getResponse.IsSuccessStatusCode);
        Assert.NotNull(getResponseData);
        Assert.Contains("Invalid Credentials", getResponseData.Message);

        // 不需要添加到清理列表，因为已经删除了
    }

    [Fact]
    public async Task DeleteRole_InvalidId_ShouldFail()
    {
        // Arrange
        var invalidRoleId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.DeleteAsync($"/api/roles/{invalidRoleId}");

        // Assert
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(responseData);
        Assert.Contains("角色不存在", responseData.Message);
    }

    [Fact]
    public async Task CreateRole_WithoutAuthentication_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var createRequest = CreateTestRoleRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", createRequest);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
    public async Task GetAllRoles_WithoutAuthentication_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);

        // Act
        var response = await _client.GetAsync("/api/roles");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
    public async Task UpdateRole_WithoutAuthentication_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var updateRequest = new UpdateRoleInfoRequest(
            RoleId: new RoleId(Guid.NewGuid()),
            Name: "测试",
            Description: "测试",
            PermissionCodes: new[] { "test.read" }
        );

        // Act
        var response = await _client.PutAsNewtonsoftJsonAsync("/api/roles/update", updateRequest);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
    public async Task DeleteRole_WithoutAuthentication_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var roleId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.DeleteAsync($"/api/roles/{roleId}");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }
}
