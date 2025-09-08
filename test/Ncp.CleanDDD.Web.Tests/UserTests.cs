using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Endpoints.UserEndpoints;
using Ncp.CleanDDD.Web.Tests.Extensions;
using NetCorePal.Extensions.Dto;
using Shouldly;
using System.Net.Http.Headers;
using FastEndpoints.Testing;
using FastEndpoints;

namespace Ncp.CleanDDD.Web.Tests;

[Collection("web")]
public class UserTests : IDisposable
{
    private readonly HttpClient _client;
    private string? _authToken;
    private string? _refreshToken;
    private UserId? _testUserId;
    private readonly List<UserId> _createdUserIds = [];

    // 测试数据常量
    private const string TestPassword = "Test123!";
    private const string TestPhone = "13800138000";
    private const string TestRealName = "测试用户";
    private const int TestStatus = 1;
    private const string TestGender = "男";
    private const int TestAge = 25;

    public UserTests(MyWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder => { builder.ConfigureServices(_ => { }); })
            .CreateClient();

        // 在构造函数中登录获取token
        LoginAndGetToken().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        // 清理测试过程中创建的用户
        CleanupTestUsers().GetAwaiter().GetResult();
    }

    private async Task CleanupTestUsers()
    {
        if (_authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            foreach (var userId in _createdUserIds)
            {
                try
                {
                    var deleteRequest = new DeleteUserRequest(userId);

                    // 使用FastEndpoints的强类型扩展方法
                    await _client.DELETEAsync<DeleteUserEndpoint, DeleteUserRequest, ResponseData<bool>>(deleteRequest);
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
                _testUserId = responseData.Data.UserId;

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

    private RegisterRequest CreateTestRegisterRequest(string name, string email, string phone = TestPhone)
    {
        return new RegisterRequest(
            Name: name,
            Email: email,
            Password: TestPassword,
            Phone: phone,
            RealName: TestRealName,
            Status: TestStatus,
            Gender: TestGender,
            Age: TestAge,
            BirthDate: DateTimeOffset.Now.AddYears(-TestAge),
            OrganizationUnitId: null,
            OrganizationUnitName: null,
            RoleIds: new List<RoleId>()
        );
    }

    [Fact, Priority(1)]
    public async Task Register_NewUser_ShouldSucceed()
    {
        // Arrange
        SetAuthHeader(false);
        var uniqueName = $"testuser_{Guid.NewGuid():N}";
        var uniqueEmail = $"test_{Guid.NewGuid():N}@example.com";
        var registerRequest = CreateTestRegisterRequest(uniqueName, uniqueEmail);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.POSTAsync<RegisterEndpoint, RegisterRequest, ResponseData<RegisterResponse>>(registerRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(uniqueName);
        result.Data.Email.ShouldBe(uniqueEmail);

        // 记录创建的用户ID用于清理
        _createdUserIds.Add(result.Data.UserId);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(2)]
    public async Task Register_WithDuplicateUsername_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var registerRequest = CreateTestRegisterRequest("admin", "admin2@example.com", "13800138001");

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.POSTAsync<RegisterEndpoint, RegisterRequest, ResponseData<RegisterResponse>>(registerRequest);

        // Assert - 使用Shouldly断言
        result.ShouldNotBeNull();
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.Code.ShouldBe(400);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(3)]
    public async Task GetAllUsers_ShouldSucceed()
    {
        // Arrange
        var queryInput = new UserQueryInput
        {
            PageIndex = 1,
            PageSize = 10,
            Keyword = null,
            Status = null,
            OrganizationUnitId = null,
            CountTotal = true
        };

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.GETAsync<GetAllUsersEndpoint, UserQueryInput, ResponseData<PagedData<UserInfoQueryDto>>>(queryInput);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Items.ShouldNotBeNull();
        result.Data.Total.ShouldBeGreaterThan(0);
    }

    [Fact, Priority(4)]
    public async Task UpdateUser_ShouldSucceed()
    {
        // Arrange
        var userId = await CreateTestUserForProfile();
        var updateRequest = new UpdateUserRequest(
            UserId: userId,
            Name: "updateduser",
            Email: "updated@example.com",
            Phone: "13900139000",
            RealName: "更新后的用户",
            Status: 1,
            Gender: "女",
            Age: 26,
            BirthDate: DateTimeOffset.Now.AddYears(-26),
            OrganizationUnitId: new OrganizationUnitId(1),
            OrganizationUnitName: "测试部门",
            Password: "NewPassword123!"
        );

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.PUTAsync<UpdateUserEndpoint, UpdateUserRequest, ResponseData<UpdateUserResponse>>(updateRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe("updateduser");
        result.Data.Email.ShouldBe("updated@example.com");
    }

    [Fact, Priority(5)]
    public async Task RefreshToken_ShouldSucceed()
    {
        // Arrange
        if (_refreshToken == null)
        {
            await LoginAndGetToken();
        }

        var refreshRequest = new RefreshTokenRequest(_refreshToken!);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.POSTAsync<RefreshTokenEndpoint, RefreshTokenRequest, ResponseData<RefreshTokenResponse>>(refreshRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Token.ShouldNotBeNull();
        result.Data.RefreshToken.ShouldNotBeNull();
        result.Data.RefreshToken.ShouldNotBe(_refreshToken);
    }

    [Fact, Priority(6)]
    public async Task DeleteUser_ShouldSucceed()
    {
        // Arrange
        var testUserId = await CreateTestUserForDeletion();
        var deleteRequest = new DeleteUserRequest(testUserId);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.DELETEAsync<DeleteUserEndpoint,DeleteUserRequest, ResponseData<bool>>(deleteRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldBeTrue();
    }

    [Fact, Priority(7)]
    public async Task UpdateUserRoles_ShouldSucceed()
    {
        // Arrange
        SetAuthHeader(false);
        var uniqueName = $"update_user_roles_{Guid.NewGuid():N}";
        var uniqueEmail = $"update_user_roles_{Guid.NewGuid():N}@example.com";
        var registerRequest = CreateTestRegisterRequest(uniqueName, uniqueEmail, "13800138003");

        var (registerResponse, registerResult) = await _client.POSTAsync<RegisterEndpoint, RegisterRequest, ResponseData<RegisterResponse>>(registerRequest);
        registerResult.ShouldNotBeNull();

        SetAuthHeader(true);
        var roleIds = new List<RoleId> { new(Guid.NewGuid()) };
        var updateRolesRequest = new UpdateUserRolesRequest(registerResult.Data.UserId, roleIds);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await _client.PUTAsync<UpdateUserRolesEndpoint, UpdateUserRolesRequest, ResponseData<UpdateUserRolesResponse>>(updateRolesRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.UserId.ShouldBe(registerResult.Data.UserId);

        // 记录创建的用户ID用于清理
        _createdUserIds.Add(registerResult.Data.UserId);
    }

    // 辅助方法：创建测试用户
    private async Task<UserId> CreateTestUserForProfile()
    {
        SetAuthHeader(false);

        var uniqueName = $"profiletest_{Guid.NewGuid():N}";
        var registerRequest = CreateTestRegisterRequest(uniqueName, "profiletest@example.com", "13800138002");

        var (response, result) = await _client.POSTAsync<RegisterEndpoint, RegisterRequest, ResponseData<RegisterResponse>>(registerRequest);

        result.ShouldNotBeNull();

        SetAuthHeader(true);
        var testUserId = result.Data.UserId;
        _createdUserIds.Add(testUserId);

        return testUserId;
    }

    private async Task<UserId> CreateTestUserForDeletion()
    {
        SetAuthHeader(false);

        var uniqueName = $"deletetest_{Guid.NewGuid():N}";
        var registerRequest = CreateTestRegisterRequest(uniqueName, "deletetest@example.com", "13800138003");

        var (response, result) = await _client.POSTAsync<RegisterEndpoint, RegisterRequest, ResponseData<RegisterResponse>>(registerRequest);

        SetAuthHeader(true);

        result.ShouldNotBeNull();
        _createdUserIds.Add(result.Data.UserId);

        return result.Data.UserId;
    }
}
