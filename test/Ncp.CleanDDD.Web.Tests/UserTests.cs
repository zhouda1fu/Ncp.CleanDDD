using Microsoft.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Endpoints.UserEndpoints;
using Ncp.CleanDDD.Web.Tests.Extensions;
using NetCorePal.Extensions.Dto;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Ncp.CleanDDD.Web.Tests;

[Collection("web")]
public class UserTests : IDisposable
{
    private readonly HttpClient _client;
    private string? _authToken;
    private string? _refreshToken;
    private UserId? _testUserId;
    private readonly List<UserId> _createdUserIds = new();

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
                    await _client.DeleteAsync($"/api/users/{userId}");
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
            BirthDate: DateTime.Now.AddYears(-TestAge),
            OrganizationUnitId: null,
            OrganizationUnitName: null,
            RoleIds: new List<RoleId>()
        );
    }

    [Fact]
    public async Task Register_NewUser_ShouldSucceed()
    {
        // Arrange
        SetAuthHeader(false);
        var uniqueName = $"testuser_{Guid.NewGuid():N}";
        var uniqueEmail = $"test_{Guid.NewGuid():N}@example.com";
        var registerRequest = CreateTestRegisterRequest(uniqueName, uniqueEmail);

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/register", registerRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal(uniqueName, responseData.Data.Name);
        Assert.Equal(uniqueEmail, responseData.Data.Email);

        // 记录创建的用户ID用于清理
        _createdUserIds.Add(responseData.Data.UserId);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var registerRequest = CreateTestRegisterRequest("admin", "admin2@example.com", "13800138001");

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/register", registerRequest);

        // Assert
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();
        Assert.NotNull(responseData);
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(400, responseData.Code);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
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

        // Act
        var queryString = $"?pageIndex={queryInput.PageIndex}&pageSize={queryInput.PageSize}&countTotal={queryInput.CountTotal}";
        var response = await _client.GetAsync($"/api/users{queryString}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<PagedData<UserInfoQueryDto>>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.NotNull(responseData.Data.Items);
        Assert.True(responseData.Data.Total > 0);
    }

    [Fact]
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
            BirthDate: DateTime.Now.AddYears(-26),
            OrganizationUnitId: new OrganizationUnitId(1),
            OrganizationUnitName: "测试部门",
            Password: "NewPassword123!"
        );

        // Act
        var response = await _client.PutAsNewtonsoftJsonAsync("/api/user/update", updateRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<UpdateUserResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal("updateduser", responseData.Data.Name);
        Assert.Equal("updated@example.com", responseData.Data.Email);
    }

    [Fact]
    public async Task RefreshToken_ShouldSucceed()
    {
        // Arrange
        if (_refreshToken == null)
        {
            await LoginAndGetToken();
        }

        var refreshRequest = new RefreshTokenRequest(_refreshToken!);

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/refresh-token", refreshRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RefreshTokenResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.NotNull(responseData.Data.Token);
        Assert.NotNull(responseData.Data.RefreshToken);
        Assert.NotEqual(_refreshToken, responseData.Data.RefreshToken);
    }

    [Fact]
    public async Task DeleteUser_ShouldSucceed()
    {
        // Arrange
        var testUserId = await CreateTestUserForDeletion();

        // Act
        var response = await _client.DeleteAsync($"/api/users/{testUserId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(responseData);
        Assert.True(responseData.Data);
    }

    [Fact]
    public async Task UpdateUserRoles_ShouldSucceed()
    {
        // Arrange
        var uniqueName = $"update_user_roles_{Guid.NewGuid():N}";
        var uniqueEmail = $"update_user_roles_{Guid.NewGuid():N}@example.com";
        var registerRequest = CreateTestRegisterRequest(uniqueName, uniqueEmail, "13800138003");

        var registerResponse = await _client.PostAsJsonAsync("/api/user/register", registerRequest);
        var registerResponseData = await registerResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();
        Assert.NotNull(registerResponseData);

        var roleIds = new List<RoleId> { new RoleId(Guid.NewGuid()) };
        var updateRolesRequest = new UpdateUserRolesRequest(registerResponseData.Data.UserId, roleIds);

        // Act
        var response = await _client.PutAsNewtonsoftJsonAsync("/api/users/update-roles", updateRolesRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<UpdateUserRolesResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal(registerResponseData.Data.UserId, responseData.Data.UserId);

        // 记录创建的用户ID用于清理
        _createdUserIds.Add(registerResponseData.Data.UserId);
    }

    // 辅助方法：创建测试用户
    private async Task<UserId> CreateTestUserForProfile()
    {
        SetAuthHeader(false);

        var uniqueName = $"profiletest_{Guid.NewGuid():N}";
        var registerRequest = CreateTestRegisterRequest(uniqueName, "profiletest@example.com", "13800138002");

        var response = await _client.PostAsJsonAsync("/api/user/register", registerRequest);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();

        Assert.NotNull(responseData);
        
        SetAuthHeader(true);
        _testUserId = responseData.Data.UserId;
        _createdUserIds.Add(_testUserId);
        
        return _testUserId;
    }

    private async Task<UserId> CreateTestUserForDeletion()
    {
        SetAuthHeader(false);

        var uniqueName = $"deletetest_{Guid.NewGuid():N}";
        var registerRequest = CreateTestRegisterRequest(uniqueName, "deletetest@example.com", "13800138003");

        var response = await _client.PostAsJsonAsync("/api/user/register", registerRequest);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();

        SetAuthHeader(true);

        Assert.NotNull(responseData);
        _createdUserIds.Add(responseData.Data.UserId);

        return responseData.Data.UserId;
    }
}