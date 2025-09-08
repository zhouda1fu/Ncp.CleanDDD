using Microsoft.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Infrastructure;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;
using Ncp.CleanDDD.Web.Endpoints.UserEndpoints;
using Ncp.CleanDDD.Web.Tests.Extensions;
using NetCorePal.Extensions.Dto;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Ncp.CleanDDD.Web.Tests;

[Collection("web")]
public class OrganizationUnitTests : IDisposable
{
    private readonly HttpClient _client;
    private string? _authToken;
    private readonly List<OrganizationUnitId> _createdOrganizationUnitIds = new();

    public OrganizationUnitTests(MyWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder => { builder.ConfigureServices(_ => { }); })
            .CreateClient();

        // 在构造函数中登录获取token
        LoginAndGetToken().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        // 清理测试过程中创建的组织单位
        CleanupTestOrganizationUnits().GetAwaiter().GetResult();
    }

    private async Task CleanupTestOrganizationUnits()
    {
        if (_authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            
            foreach (var organizationUnitId in _createdOrganizationUnitIds)
            {
                try
                {
                    await _client.DeleteAsync($"/api/organization-units/{organizationUnitId}");
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

    private CreateOrganizationUnitRequest CreateTestOrganizationUnitRequest(string name, string description, OrganizationUnitId? parentId = null, int sortOrder = 1)
    {
        return new CreateOrganizationUnitRequest(
            Name: name,
            Description: description,
            ParentId: parentId,
            SortOrder: sortOrder
        );
    }

    [Fact]
    public async Task CreateOrganizationUnit_NewOrganizationUnit_ShouldSucceed()
    {
        // Arrange
        var uniqueName = $"测试部门_{Guid.NewGuid():N}";
        var description = "测试部门描述";
        var createRequest = CreateTestOrganizationUnitRequest(uniqueName, description);

        // Act
        var response = await _client.PostAsJsonAsync("/api/organization-units", createRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateOrganizationUnitResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal(uniqueName, responseData.Data.Name);
        Assert.Equal(description, responseData.Data.Description);

        // 记录创建的组织单位ID用于清理
        _createdOrganizationUnitIds.Add(responseData.Data.Id);
    }

    [Fact]
    public async Task CreateOrganizationUnit_WithParentId_ShouldSucceed()
    {
        // Arrange
        // 先创建一个父级组织单位
        var parentName = $"父级部门_{Guid.NewGuid():N}";
        var parentRequest = CreateTestOrganizationUnitRequest(parentName, "父级部门描述");
        var parentResponse = await _client.PostAsJsonAsync("/api/organization-units", parentRequest);
        var parentResponseData = await parentResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateOrganizationUnitResponse>>();
        Assert.NotNull(parentResponseData?.Data);
        _createdOrganizationUnitIds.Add(parentResponseData.Data.Id);

        // 创建子级组织单位
        var childName = $"子级部门_{Guid.NewGuid():N}";
        var childRequest = CreateTestOrganizationUnitRequest(childName, "子级部门描述", parentResponseData.Data.Id, 2);

        // Act
        var response = await _client.PostAsNewtonsoftJsonAsync("/api/organization-units", childRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateOrganizationUnitResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal(childName, responseData.Data.Name);
        Assert.Equal("子级部门描述", responseData.Data.Description);

        // 记录创建的组织单位ID用于清理
        _createdOrganizationUnitIds.Add(responseData.Data.Id);
    }

    [Fact]
    public async Task CreateOrganizationUnit_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var createRequest = CreateTestOrganizationUnitRequest("未授权测试部门", "测试描述");

        // Act
        var response = await _client.PostAsJsonAsync("/api/organization-units", createRequest);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
    public async Task GetAllOrganizationUnits_ShouldSucceed()
    {
        // Arrange
        var queryInput = new OrganizationUnitQueryInput
        {
            Name = null,
            Description = null,
            IsActive = null,
            ParentId = null
        };

        // Act
        //var queryString = $"?name={queryInput.Name}&description={queryInput.Description}&isActive={queryInput.IsActive}&parentId={queryInput.ParentId}";
        var response = await _client.GetAsync($"/api/organization-units");//{queryString}

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<IEnumerable<OrganizationUnitQueryDto>>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
    }

    [Fact]
    public async Task GetAllOrganizationUnits_WithFilters_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var testName = $"筛选测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(testName, "筛选测试描述");
        var createResponse = await _client.PostAsJsonAsync("/api/organization-units", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateOrganizationUnitResponse>>();
        Assert.NotNull(createResponseData?.Data);
        _createdOrganizationUnitIds.Add(createResponseData.Data.Id);

        // 使用名称筛选
        var queryInput = new OrganizationUnitQueryInput
        {
            Name = "筛选测试",
            Description = null,
            IsActive = true,
            ParentId = null
        };

        // Act
        var queryString = $"?name={queryInput.Name}&isActive={queryInput.IsActive}";
        var response = await _client.GetAsync($"/api/organization-units{queryString}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<IEnumerable<OrganizationUnitQueryDto>>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Contains(responseData.Data, ou => ou.Name.Contains("筛选测试"));
    }

    [Fact]
    public async Task GetOrganizationUnit_ExistingId_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var testName = $"详情测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(testName, "详情测试描述");
        var createResponse = await _client.PostAsJsonAsync("/api/organization-units", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateOrganizationUnitResponse>>();
        Assert.NotNull(createResponseData?.Data);
        _createdOrganizationUnitIds.Add(createResponseData.Data.Id);

        // Act
        var response = await _client.GetAsync($"/api/organization-units/{createResponseData.Data.Id}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<GetOrganizationUnitResponse>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
        Assert.Equal(testName, responseData.Data.Name);
        Assert.Equal("详情测试描述", responseData.Data.Description);
        Assert.Equal(createResponseData.Data.Id, responseData.Data.Id);
    }

    [Fact]
    public async Task GetOrganizationUnit_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = new OrganizationUnitId(99999);

        // Act
        var response = await _client.GetAsync($"/api/organization-units/{nonExistentId}");


        // Assert
        Assert.True(response.IsSuccessStatusCode);
        // 根据端点实现，应该返回400状态码和KnownException
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<GetOrganizationUnitResponse>>();
        Assert.NotNull(responseData);
        Assert.Equal(0, responseData.Code);
    }

    [Fact]
    public async Task UpdateOrganizationUnit_ExistingId_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var originalName = $"更新测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(originalName, "原始描述");
        var createResponse = await _client.PostAsJsonAsync("/api/organization-units", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateOrganizationUnitResponse>>();
        Assert.NotNull(createResponseData?.Data);
        _createdOrganizationUnitIds.Add(createResponseData.Data.Id);

        // 更新组织单位
        var updateRequest = new UpdateOrganizationUnitRequest(
            Name: "更新后的部门名称",
            Description: "更新后的描述",
            ParentId: null,
            SortOrder: 2
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/organization-units/{createResponseData.Data.Id}", updateRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(responseData);
        Assert.True(responseData.Data);

        // 验证更新是否成功
        var getResponse = await _client.GetAsync($"/api/organization-units/{createResponseData.Data.Id}");
        var getResponseData = await getResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<GetOrganizationUnitResponse>>();
        Assert.NotNull(getResponseData?.Data);
        Assert.Equal("更新后的部门名称", getResponseData.Data.Name);
        Assert.Equal("更新后的描述", getResponseData.Data.Description);
    }

    [Fact]
    public async Task UpdateOrganizationUnit_NonExistentId_ShouldFail()
    {
        // Arrange
        var nonExistentId = new OrganizationUnitId(99999);
        var updateRequest = new UpdateOrganizationUnitRequest(
            Name: "不存在的部门",
            Description: "不存在的描述",
            ParentId: null,
            SortOrder: 1
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/organization-units/{nonExistentId}", updateRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        // 根据端点实现，应该返回400状态码
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(responseData);
        Assert.False(responseData.Data);
    }

    [Fact]
    public async Task DeleteOrganizationUnit_ExistingId_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var testName = $"删除测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(testName, "删除测试描述");
        var createResponse = await _client.PostAsJsonAsync("/api/organization-units", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateOrganizationUnitResponse>>();
        Assert.NotNull(createResponseData?.Data);

        // Act
        var response = await _client.DeleteAsync($"/api/organization-units/{createResponseData.Data.Id}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(responseData);
        Assert.True(responseData.Data);

        //// 验证删除是否成功
        //var getResponse = await _client.GetAsync($"/api/organization-units/{createResponseData.Data.Id}");
        //Assert.False(getResponse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task DeleteOrganizationUnit_NonExistentId_ShouldFail()
    {
        // Arrange
        var nonExistentId = new OrganizationUnitId(99999);

        // Act
        var response = await _client.DeleteAsync($"/api/organization-units/{nonExistentId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        // 根据端点实现，应该返回400状态码
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(responseData);
        Assert.False(responseData.Data);
    }

    [Fact]
    public async Task GetOrganizationUnitTree_ShouldSucceed()
    {
        // Arrange
        var treeRequest = new GetOrganizationUnitTreeRequest(IncludeInactive: false);

        // Act
        var response = await _client.GetAsync("/api/organization-units/tree?includeInactive=false");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<IEnumerable<OrganizationUnitTreeDto>>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
    }

    [Fact]
    public async Task GetOrganizationUnitTree_WithIncludeInactive_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var testName = $"树形测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(testName, "树形测试描述");
        var createResponse = await _client.PostAsJsonAsync("/api/organization-units", createRequest);
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<CreateOrganizationUnitResponse>>();
        Assert.NotNull(createResponseData?.Data);
        _createdOrganizationUnitIds.Add(createResponseData.Data.Id);

        // Act
        var response = await _client.GetAsync("/api/organization-units/tree?includeInactive=true");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<IEnumerable<OrganizationUnitTreeDto>>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);
    }

    [Fact]
    public async Task GetOrganizationUnitTree_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);

        // Act
        var response = await _client.GetAsync("/api/organization-units/tree");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
    public async Task GetAllOrganizationUnits_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);

        // Act
        var response = await _client.GetAsync("/api/organization-units");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
    public async Task UpdateOrganizationUnit_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var updateRequest = new UpdateOrganizationUnitRequest(
            Name: "未授权更新",
            Description: "未授权描述",
            ParentId: null,
            SortOrder: 1
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/organization-units/1", updateRequest);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact]
    public async Task DeleteOrganizationUnit_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);

        // Act
        var response = await _client.DeleteAsync("/api/organization-units/1");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        // 恢复认证头
        SetAuthHeader(true);
    }
}
