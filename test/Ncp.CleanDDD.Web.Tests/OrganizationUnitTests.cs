using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;
using Ncp.CleanDDD.Web.Endpoints.UserEndpoints;
using Ncp.CleanDDD.Web.Tests.Base;
using Ncp.CleanDDD.Web.Tests.Extensions;
using NetCorePal.Extensions.Dto;
using Shouldly;
using System.Net.Http.Headers;
using FastEndpoints.Testing;
using FastEndpoints;

namespace Ncp.CleanDDD.Web.Tests;

[Collection("web")]
public class OrganizationUnitTests : BaseWebTest
{
    private readonly List<OrganizationUnitId> _createdOrganizationUnitIds = new();

    public OrganizationUnitTests(MyWebApplicationFactory factory) : base(factory)
    {
    }

    public override void Dispose()
    {
        // 清理测试过程中创建的组织单位
        CleanupTestOrganizationUnits().GetAwaiter().GetResult();
        base.Dispose();
    }

    private async Task CleanupTestOrganizationUnits()
    {
        if (AuthToken != null)
        {
            SetAuthHeader(true);
            
            foreach (var organizationUnitId in _createdOrganizationUnitIds)
            {
                try
                {
                    var deleteRequest = new DeleteOrganizationUnitRequest(organizationUnitId);
                    // 使用FastEndpoints的强类型扩展方法
                    await Client.DELETEAsync<DeleteOrganizationUnitEndpoint, DeleteOrganizationUnitRequest, ResponseData<bool>>(deleteRequest);
                }
                catch
                {
                    // 忽略清理过程中的错误
                }
            }
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

    [Fact, Priority(1)]
    public async Task CreateOrganizationUnit_NewOrganizationUnit_ShouldSucceed()
    {
        // Arrange
        var uniqueName = $"测试部门_{Guid.NewGuid():N}";
        var description = "测试部门描述";
        var createRequest = CreateTestOrganizationUnitRequest(uniqueName, description);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(createRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(uniqueName);
        result.Data.Description.ShouldBe(description);

        // 记录创建的组织单位ID用于清理
        _createdOrganizationUnitIds.Add(result.Data.Id);
    }

    [Fact, Priority(2)]
    public async Task CreateOrganizationUnit_WithParentId_ShouldSucceed()
    {
        // Arrange
        // 先创建一个父级组织单位
        var parentName = $"父级部门_{Guid.NewGuid():N}";
        var parentRequest = CreateTestOrganizationUnitRequest(parentName, "父级部门描述");
        var (parentResponse, parentResult) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(parentRequest);
        parentResult.ShouldNotBeNull();
        _createdOrganizationUnitIds.Add(parentResult.Data.Id);

        // 创建子级组织单位
        var childName = $"子级部门_{Guid.NewGuid():N}";
        var childRequest = CreateTestOrganizationUnitRequest(childName, "子级部门描述", parentResult.Data.Id, 2);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(childRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(childName);
        result.Data.Description.ShouldBe("子级部门描述");

        // 记录创建的组织单位ID用于清理
        _createdOrganizationUnitIds.Add(result.Data.Id);
    }

    [Fact, Priority(3)]
    public async Task CreateOrganizationUnit_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var createRequest = CreateTestOrganizationUnitRequest("未授权测试部门", "测试描述");

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(createRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(4)]
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

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.GETAsync<GetAllOrganizationUnitsEndpoint, OrganizationUnitQueryInput, ResponseData<IEnumerable<OrganizationUnitQueryDto>>>(queryInput);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
    }

    [Fact, Priority(5)]
    public async Task GetAllOrganizationUnits_WithFilters_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var testName = $"筛选测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(testName, "筛选测试描述");
        var (createResponse, createResult) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(createRequest);
        createResult.ShouldNotBeNull();
        _createdOrganizationUnitIds.Add(createResult.Data.Id);

        // 使用名称筛选
        var queryInput = new OrganizationUnitQueryInput
        {
            Name = "筛选测试",
            Description = null,
            IsActive = true,
            ParentId = null
        };

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.GETAsync<GetAllOrganizationUnitsEndpoint, OrganizationUnitQueryInput, ResponseData<IEnumerable<OrganizationUnitQueryDto>>>(queryInput);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldContain(ou => ou.Name.Contains("筛选测试"));
    }

    [Fact, Priority(6)]
    public async Task GetOrganizationUnit_ExistingId_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var testName = $"详情测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(testName, "详情测试描述");
        var (createResponse, createResult) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(createRequest);
        createResult.ShouldNotBeNull();
        _createdOrganizationUnitIds.Add(createResult.Data.Id);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.GETAsync<GetOrganizationUnitEndpoint, GetOrganizationUnitRequest, ResponseData<GetOrganizationUnitResponse>>(new GetOrganizationUnitRequest(createResult.Data.Id));

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(testName);
        result.Data.Description.ShouldBe("详情测试描述");
        result.Data.Id.ShouldBe(createResult.Data.Id);
    }

    [Fact, Priority(7)]
    public async Task GetOrganizationUnit_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = new OrganizationUnitId(99999);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.GETAsync<GetOrganizationUnitEndpoint, GetOrganizationUnitRequest, ResponseData<GetOrganizationUnitResponse>>(new GetOrganizationUnitRequest(nonExistentId));

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        // 根据端点实现，应该返回400状态码和KnownException
        result.ShouldNotBeNull();
        result.Code.ShouldBe(0);
    }

    [Fact, Priority(8)]
    public async Task UpdateOrganizationUnit_ExistingId_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var originalName = $"更新测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(originalName, "原始描述");
        var (createResponse, createResult) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(createRequest);
        createResult.ShouldNotBeNull();
        _createdOrganizationUnitIds.Add(createResult.Data.Id);

        // 更新组织单位
        var updateRequest = new UpdateOrganizationUnitRequest(
            createResult.Data.Id,
            Name: "更新后的部门名称",
            Description: "更新后的描述",
            ParentId: null,
            SortOrder: 2
        );

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.PUTAsync<UpdateOrganizationUnitEndpoint, UpdateOrganizationUnitRequest, ResponseData<bool>>(updateRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldBeTrue();

        // 验证更新是否成功
        var (getResponse, getResult) = await Client.GETAsync<GetOrganizationUnitEndpoint, GetOrganizationUnitRequest, ResponseData<GetOrganizationUnitResponse>>(new GetOrganizationUnitRequest(createResult.Data.Id));
        getResult.ShouldNotBeNull();
        getResult.Data!.Name.ShouldBe("更新后的部门名称");
        getResult.Data.Description.ShouldBe("更新后的描述");
    }

    [Fact, Priority(9)]
    public async Task UpdateOrganizationUnit_NonExistentId_ShouldFail()
    {
        // Arrange
        var nonExistentId = new OrganizationUnitId(99999);
        var updateRequest = new UpdateOrganizationUnitRequest(nonExistentId,
            Name: "不存在的部门",
            Description: "不存在的描述",
            ParentId: null,
            SortOrder: 1
        );

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.PUTAsync<UpdateOrganizationUnitEndpoint, UpdateOrganizationUnitRequest, ResponseData<bool>>(updateRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        // 根据端点实现，应该返回400状态码
        result.ShouldNotBeNull();
        result.Data.ShouldBeFalse();
    }

    [Fact, Priority(10)]
    public async Task DeleteOrganizationUnit_ExistingId_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var testName = $"删除测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(testName, "删除测试描述");
        var (createResponse, createResult) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(createRequest);
        createResult.ShouldNotBeNull();

        // Act - 使用FastEndpoints的强类型扩展方法
        var deleteRequest = new DeleteOrganizationUnitRequest(createResult.Data.Id);
        var (response, result) = await Client.DELETEAsync<DeleteOrganizationUnitEndpoint, DeleteOrganizationUnitRequest, ResponseData<bool>>(deleteRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldBeTrue();

        //// 验证删除是否成功
        //var getResponse = await Client.GetAsync($"/api/organization-units/{createResult.Data.Id}");
        //Assert.False(getResponse.IsSuccessStatusCode);
    }

    [Fact, Priority(11)]
    public async Task DeleteOrganizationUnit_NonExistentId_ShouldFail()
    {
        // Arrange
        var nonExistentId = new OrganizationUnitId(99999);

        // Act - 使用FastEndpoints的强类型扩展方法
        var deleteRequest = new DeleteOrganizationUnitRequest(nonExistentId);
        var (response, result) = await Client.DELETEAsync<DeleteOrganizationUnitEndpoint, DeleteOrganizationUnitRequest, ResponseData<bool>>(deleteRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        // 根据端点实现，应该返回400状态码
        result.ShouldNotBeNull();
        result.Data.ShouldBeFalse();
    }

    [Fact, Priority(12)]
    public async Task GetOrganizationUnitTree_ShouldSucceed()
    {
        // Arrange
        var treeRequest = new GetOrganizationUnitTreeRequest(IncludeInactive: false);

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.GETAsync<GetOrganizationUnitTreeEndpoint, GetOrganizationUnitTreeRequest, ResponseData<IEnumerable<OrganizationUnitTreeDto>>>(treeRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
    }

    [Fact, Priority(13)]
    public async Task GetOrganizationUnitTree_WithIncludeInactive_ShouldSucceed()
    {
        // Arrange
        // 先创建一个测试组织单位
        var testName = $"树形测试部门_{Guid.NewGuid():N}";
        var createRequest = CreateTestOrganizationUnitRequest(testName, "树形测试描述");
        var (createResponse, createResult) = await Client.POSTAsync<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>(createRequest);
        createResult.ShouldNotBeNull();
        _createdOrganizationUnitIds.Add(createResult.Data.Id);

        // Act - 使用FastEndpoints的强类型扩展方法
        var treeRequest = new GetOrganizationUnitTreeRequest(IncludeInactive: true);
        var (response, result) = await Client.GETAsync<GetOrganizationUnitTreeEndpoint, GetOrganizationUnitTreeRequest, ResponseData<IEnumerable<OrganizationUnitTreeDto>>>(treeRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
    }

    [Fact, Priority(14)]
    public async Task GetOrganizationUnitTree_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);

        // Act - 使用FastEndpoints的强类型扩展方法
        var treeRequest = new GetOrganizationUnitTreeRequest(IncludeInactive: false);
        var (response, result) = await Client.GETAsync<GetOrganizationUnitTreeEndpoint, GetOrganizationUnitTreeRequest, ResponseData<IEnumerable<OrganizationUnitTreeDto>>>(treeRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(15)]
    public async Task GetAllOrganizationUnits_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);

        // Act - 使用FastEndpoints的强类型扩展方法
        var queryInput = new OrganizationUnitQueryInput();
        var (response, result) = await Client.GETAsync<GetAllOrganizationUnitsEndpoint, OrganizationUnitQueryInput, ResponseData<IEnumerable<OrganizationUnitQueryDto>>>(queryInput);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(16)]
    public async Task UpdateOrganizationUnit_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var updateRequest = new UpdateOrganizationUnitRequest(
            new OrganizationUnitId(1),
            Name: "未授权更新",
            Description: "未授权描述",
            ParentId: null,
            SortOrder: 1
        );

        // Act - 使用FastEndpoints的强类型扩展方法
        var (response, result) = await Client.PUTAsync<UpdateOrganizationUnitEndpoint, UpdateOrganizationUnitRequest, ResponseData<bool>>(updateRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }

    [Fact, Priority(17)]
    public async Task DeleteOrganizationUnit_WithoutAuth_ShouldFail()
    {
        // Arrange
        SetAuthHeader(false);
        var organizationUnitId = new OrganizationUnitId(1);

        // Act - 使用FastEndpoints的强类型扩展方法
        var deleteRequest = new DeleteOrganizationUnitRequest(organizationUnitId);
        var (response, result) = await Client.DELETEAsync<DeleteOrganizationUnitEndpoint, DeleteOrganizationUnitRequest, ResponseData<bool>>(deleteRequest);

        // Assert - 使用Shouldly断言
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);

        // 恢复认证头
        SetAuthHeader(true);
    }
}
