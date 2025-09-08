using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;
using Ncp.CleanDDD.Web.Endpoints.UserEndpoints;
using Ncp.CleanDDD.Web.Tests.Extensions;
using NetCorePal.Extensions.Dto;
using Shouldly;
using System.Net.Http.Headers;
using FastEndpoints.Testing;
using FastEndpoints;

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
                    var deleteRequest = new DeleteRoleRequest(roleId);
                    // 使用FastEndpoints的强类型扩展方法
                    await _client.DELETEAsync<DeleteRoleEndpoint, DeleteRoleRequest, ResponseData<bool>>(deleteRequest);
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

    [Fact, Priority(1)]
    public async Task CreateRole_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var uniqueName = $"创建角色测试_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(uniqueName, "创建角色测试描述");

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(uniqueName);
        result.Data.Description.ShouldBe("创建角色测试描述");
        _createdRoleIds.Add(result.Data.RoleId);
    }

    [Fact, Priority(2)]
    public async Task CreateRole_DuplicateName_ShouldFail()
    {
        // Arrange
        var roleName = $"重复角色名_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(roleName);

        // 先创建一个角色
        var (firstResponse, firstResult) = await _client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
        firstResponse.IsSuccessStatusCode.ShouldBeTrue();
        _createdRoleIds.Add(firstResult!.Data!.RoleId);

        // Act - 尝试创建相同名称的角色
        var duplicateRequest = CreateTestRoleRequest(roleName);
        var (response, result) = await _client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(duplicateRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Code.ShouldBe(400);
        result.Message.ShouldContain("该角色已存在");
    }

    [Fact, Priority(3)]
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

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.GETAsync<GetAllRoleEndpoint, RoleQueryInput, ResponseData<PagedData<RoleQueryDto>>>(queryInput);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Items.ShouldNotBeNull();
    }

    [Fact, Priority(4)]
    public async Task GetAllRoles_WithNameFilter_ShouldSucceed()
    {
        // Arrange
        var testRoleName = $"筛选测试角色_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(testRoleName);
        var (createResponse, createResult) = await _client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
        _createdRoleIds.Add(createResult!.Data!.RoleId);

        // Act - 使用FastEndpoints的强类型扩展方法
        var queryInput = new RoleQueryInput
        {
            PageIndex = 1,
            PageSize = 10,
            Name = testRoleName,
            Description = null,
            IsActive = null,
            CountTotal = true
        };
        var (response, result) = await _client.GETAsync<GetAllRoleEndpoint, RoleQueryInput, ResponseData<PagedData<RoleQueryDto>>>(queryInput);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Items.ShouldNotBeNull();
        result.Data.Items.ShouldContain(r => r.Name == testRoleName);
    }

    [Fact, Priority(5)]
    public async Task GetRole_ValidId_ShouldSucceed()
    {
        // Arrange - 先创建一个角色
        var testRoleName = $"获取角色测试_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(testRoleName);
        var (createResponse, createResult) = await _client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
        createResult.ShouldNotBeNull();
        
        var roleId = createResult.Data!.RoleId;

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<GetRoleResponse>>(new GetRoleRequest(roleId));

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(testRoleName);
    }

    [Fact, Priority(6)]
    public async Task GetRole_InvalidId_ShouldFail()
    {
        // Arrange
        var invalidRoleId = new RoleId(Guid.NewGuid());

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<GetRoleResponse>>(new GetRoleRequest(invalidRoleId));

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Message.ShouldContain("Invalid Credentials");
    }

    [Fact, Priority(7)]
    public async Task UpdateRole_ValidRequest_ShouldSucceed()
    {
        // Arrange - 先创建一个角色
        var originalName = $"更新前角色_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(originalName);
        var (createResponse, createResult) = await _client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
        createResult.ShouldNotBeNull();
        
        var roleId = createResult.Data!.RoleId;
        _createdRoleIds.Add(roleId);

        var updatedName = $"更新后角色_{Guid.NewGuid():N}";
        var updateRequest = new UpdateRoleInfoRequest(
            RoleId: roleId,
            Name: updatedName,
            Description: "更新后的描述",
            PermissionCodes: new[] { "updated.read", "updated.write" }
        );

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.PUTAsync<UpdateRoleEndpoint, UpdateRoleInfoRequest, ResponseData<bool>>(updateRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldBeTrue();

        // 验证更新是否成功
        var (getResponse, getResult) = await _client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<GetRoleResponse>>(new GetRoleRequest(roleId));
        getResult.ShouldNotBeNull();
        getResult.Data!.Name.ShouldBe(updatedName);
        getResult.Data.Description.ShouldBe("更新后的描述");
    }

    [Fact, Priority(8)]
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

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.PUTAsync<UpdateRoleEndpoint, UpdateRoleInfoRequest, ResponseData<bool>>(updateRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Message.ShouldContain("未找到角色");
    }

    [Fact, Priority(9)]
    public async Task DeleteRole_ValidId_ShouldSucceed()
    {
        // Arrange - 先创建一个角色
        var testRoleName = $"待删除角色_{Guid.NewGuid():N}";
        var createRequest = CreateTestRoleRequest(testRoleName);
        var (createResponse, createResult) = await _client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
        createResult.ShouldNotBeNull();
        
        var roleId = createResult.Data!.RoleId;

        // Act - 使用FastEndpoints的强类型扩展方法
        var deleteRequest = new DeleteRoleRequest(roleId);
        var (response, result) = await _client.DELETEAsync<DeleteRoleEndpoint, DeleteRoleRequest, ResponseData<bool>>(deleteRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldBeTrue();

        // 验证角色确实被删除了
        var (getResponse, getResult) = await _client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<GetRoleResponse>>(new GetRoleRequest(roleId));

        getResponse.IsSuccessStatusCode.ShouldBeTrue();
        getResult.ShouldNotBeNull();
        getResult.Message.ShouldContain("Invalid Credentials");

        // 不需要添加到清理列表，因为已经删除了
    }

    [Fact, Priority(10)]
    public async Task DeleteRole_InvalidId_ShouldFail()
    {
        // Arrange
        var invalidRoleId = new RoleId(Guid.NewGuid());

        // Act - 使用FastEndpoints的强类型扩展方法
        var deleteRequest = new DeleteRoleRequest(invalidRoleId);
        var (response, result) = await _client.DELETEAsync<DeleteRoleEndpoint, DeleteRoleRequest, ResponseData<bool>>(deleteRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Message.ShouldContain("角色不存在");
    }

    [Fact, Priority(11)]
    public async Task CreateRole_WithoutAuthentication_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var createRequest = CreateTestRoleRequest();

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(12)]
    public async Task GetAllRoles_WithoutAuthentication_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);

        // Act - 使用FastEndpoints的强类型扩展方法
        var queryInput = new RoleQueryInput();
        var (response, result) = await _client.GETAsync<GetAllRoleEndpoint, RoleQueryInput, ResponseData<PagedData<RoleQueryDto>>>(queryInput);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(13)]
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

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.PUTAsync<UpdateRoleEndpoint, UpdateRoleInfoRequest, ResponseData<bool>>(updateRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(14)]
    public async Task DeleteRole_WithoutAuthentication_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var roleId = new RoleId(Guid.NewGuid());

        // Act - 使用FastEndpoints的强类型扩展方法
        var deleteRequest = new DeleteRoleRequest(roleId);
        var (response, result) = await _client.DELETEAsync<DeleteRoleEndpoint, DeleteRoleRequest, ResponseData<bool>>(deleteRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }
}
