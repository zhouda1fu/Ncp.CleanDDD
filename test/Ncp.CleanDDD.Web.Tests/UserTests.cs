using Microsoft.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Endpoints.UserEndpoints;
using Ncp.CleanDDD.Web.Tests.Extensions;
using NetCorePal.Extensions.Dto;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Ncp.CleanDDD.Web.Tests;

[Collection("web")]
public class UserTests
{
    private readonly HttpClient _client;
    private string? _authToken;
    private string? _refreshToken;
    private UserId? _testUserId;

    public UserTests(MyWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder => { builder.ConfigureServices(_ => { }); })
            .CreateClient();

        // 在构造函数中登录获取token
        LoginAndGetToken().GetAwaiter().GetResult();
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



    [Fact]
    public async Task Register_New_User_Should_Succeed()
    {
        // 移除认证头，因为注册端点允许匿名访问
        _client.DefaultRequestHeaders.Authorization = null;

        var registerRequest = new RegisterRequest(
            Name: "testuser",
            Email: "test@example.com",
            Password: "Test123!",
            Phone: "13800138000",
            RealName: "测试用户",
            Status: 1,
            Gender: "男",
            Age: 25,
            BirthDate: DateTime.Now.AddYears(-25),
            OrganizationUnitId: null,
            OrganizationUnitName: null,
            RoleIds: new List<RoleId>()
        );

        var response = await _client.PostAsJsonAsync("/api/user/register", registerRequest);

        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal("testuser", responseData.Data.Name);
        Assert.Equal("test@example.com", responseData.Data.Email);

        // 恢复认证头
        if (_authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }
    }

    [Fact]
    public async Task Register_With_Duplicate_Username_Should_Fail()
    {
        // 移除认证头
        _client.DefaultRequestHeaders.Authorization = null;

        var registerRequest = new RegisterRequest(
            Name: "admin", // 使用已存在的用户名
            Email: "admin2@example.com",
            Password: "Test123!",
            Phone: "13800138001",
            RealName: "管理员2",
            Status: 1,
            Gender: "男",
            Age: 30,
            BirthDate: DateTime.Now.AddYears(-30),
            OrganizationUnitId: null,
            OrganizationUnitName: null,
            RoleIds: new List<RoleId>()
        );

        var response = await _client.PostAsJsonAsync("/api/user/register", registerRequest);

        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();

        if (responseData == null)
        {
            Assert.Fail("responseData为null");
        }

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(400, responseData.Code);

        // 恢复认证头
        if (_authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }
    }

    [Fact]
    public async Task Get_All_Users_Should_Succeed()
    {
        var queryInput = new UserQueryInput
        {
            PageIndex = 1,
            PageSize = 10,
            Keyword = null,
            Status = null,
            OrganizationUnitId = null,
            CountTotal = true
        };

        // 构建查询字符串
        var queryString = $"?pageIndex={queryInput.PageIndex}&pageSize={queryInput.PageSize}&countTotal={queryInput.CountTotal}";
        var response = await _client.GetAsync($"/api/users{queryString}");

        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<PagedData<UserInfoQueryDto>>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.NotNull(responseData.Data.Items);
        Assert.True(responseData.Data.Total > 0);
    }




    [Fact]
    public async Task Update_User_Should_Succeed()
    {

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

        var response = await _client.PutAsNewtonsoftJsonAsync("/api/user/update", updateRequest);

        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<UpdateUserResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal("updateduser", responseData.Data.Name);
        Assert.Equal("updated@example.com", responseData.Data.Email);
    }



    [Fact]
    public async Task Refresh_Token_Should_Succeed()
    {
        if (_refreshToken == null)
        {
            // 如果没有refresh token，先登录获取
            await LoginAndGetToken();
        }

        var refreshRequest = new RefreshTokenRequest(_refreshToken!);
        var response = await _client.PostAsJsonAsync("/api/user/refresh-token", refreshRequest);

        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RefreshTokenResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.NotNull(responseData.Data.Token);
        Assert.NotNull(responseData.Data.RefreshToken);
        Assert.NotEqual(_refreshToken, responseData.Data.RefreshToken); // 新的refresh token应该不同
    }



    [Fact]
    public async Task Delete_User_Should_Succeed()
    {
        // 先创建一个测试用户用于删除
        var testUserId = await CreateTestUserForDeletion();
        var response = await _client.DeleteAsync($"/api/users/{testUserId}");

        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(responseData);
        Assert.True(responseData.Data);
    }



    [Fact]
    public async Task Update_User_Roles_Should_Succeed()
    {

        var registerRequest = new RegisterRequest(
         Name: "update_user_roles",
         Email: "update_user_roles@example.com",
         Password: "Test123!",
         Phone: "13800138003",
         RealName: "资料测试用户",
         Status: 1,
         Gender: "男",
         Age: 28,
         BirthDate: DateTime.Now.AddYears(-28),
         OrganizationUnitId: null,
         OrganizationUnitName: null,
         RoleIds: new List<RoleId>()
     );

        var registerResponse = await _client.PostAsJsonAsync("/api/user/register", registerRequest);
        var registerResponseData = await registerResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();

        if (registerResponseData == null)
        {
            Assert.Fail("responseData为null");
        }

        var roleIds = new List<RoleId> { new RoleId(Guid.NewGuid()) };
        var updateRolesRequest = new UpdateUserRolesRequest(registerResponseData.Data.UserId, roleIds);
        var response = await _client.PutAsNewtonsoftJsonAsync("/api/users/update-roles", updateRolesRequest);

        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<UpdateUserRolesResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal(registerResponseData.Data.UserId, responseData.Data.UserId);
    }



    // 辅助方法：创建测试用户
    private async Task<UserId> CreateTestUserForProfile()
    {
        // 移除认证头
        _client.DefaultRequestHeaders.Authorization = null;

        var registerRequest = new RegisterRequest(
            Name: "user" + Guid.NewGuid(),
            Email: "profiletest@example.com",
            Password: "Test123!",
            Phone: "13800138002",
            RealName: "资料测试用户",
            Status: 1,
            Gender: "男",
            Age: 28,
            BirthDate: DateTime.Now.AddYears(-28),
            OrganizationUnitId: null,
            OrganizationUnitName: null,
            RoleIds: new List<RoleId>()
        );

        var response = await _client.PostAsJsonAsync("/api/user/register", registerRequest);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();

        if (responseData == null)
        {
            Assert.Fail("responseData为null");
        }
        // 恢复认证头
        if (_authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }

        _testUserId = responseData.Data.UserId;
        return _testUserId;
    }

    private async Task<UserId> CreateTestUserForDeletion()
    {
        // 移除认证头
        _client.DefaultRequestHeaders.Authorization = null;

        var registerRequest = new RegisterRequest(
            Name: "deletetestuser",
            Email: "deletetest@example.com",
            Password: "Test123!",
            Phone: "13800138003",
            RealName: "删除测试用户",
            Status: 1,
            Gender: "女",
            Age: 24,
            BirthDate: DateTime.Now.AddYears(-24),
            OrganizationUnitId: null,
            OrganizationUnitName: null,
            RoleIds: new List<RoleId>()
        );

        var response = await _client.PostAsJsonAsync("/api/user/register", registerRequest);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<RegisterResponse>>();

        // 恢复认证头
        if (_authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }

        if (responseData == null)
        {
            Assert.Fail("responseData为null");
        }

        return responseData.Data.UserId;
    }
}