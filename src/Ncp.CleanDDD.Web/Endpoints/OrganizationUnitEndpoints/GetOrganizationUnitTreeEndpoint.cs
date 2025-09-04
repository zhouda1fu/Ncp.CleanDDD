using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 获取组织单位树形结构的请求模型
/// </summary>
/// <param name="IncludeInactive">是否包含非激活状态的组织单位，默认为false</param>
public record GetOrganizationUnitTreeRequest(bool IncludeInactive = false);

/// <summary>
/// 获取组织单位树形结构的API端点
/// 该端点用于查询组织单位的层级树形结构，便于前端展示组织架构
/// </summary>
[Tags("OrganizationUnits")] // API文档标签，用于Swagger文档分组
public class GetOrganizationUnitTreeEndpoint : Endpoint<GetOrganizationUnitTreeRequest, ResponseData<IEnumerable<OrganizationUnitTreeDto>?>>
{
    /// <summary>
    /// 组织单位查询服务，用于执行组织单位相关的查询操作
    /// </summary>
    private readonly OrganizationUnitQuery _organizationUnitQuery;

    /// <summary>
    /// 构造函数，通过依赖注入获取组织单位查询服务实例
    /// </summary>
    /// <param name="organizationUnitQuery">组织单位查询服务实例</param>
    public GetOrganizationUnitTreeEndpoint(OrganizationUnitQuery organizationUnitQuery)
    {
        _organizationUnitQuery = organizationUnitQuery;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询组织单位树形结构
        Get("/api/organization-units/tree");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和组织单位查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 查询组织单位的树形结构并返回结果
    /// </summary>
    /// <param name="req">包含查询选项的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetOrganizationUnitTreeRequest req, CancellationToken ct)
    {
        // 通过查询服务获取组织单位树形结构
        // 根据请求参数决定是否包含非激活状态的组织单位
        var organizationUnitTree = await _organizationUnitQuery.GetOrganizationUnitTreeAsync(req.IncludeInactive, ct);
        
        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(organizationUnitTree.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 获取组织单位树形结构端点的API文档配置
/// </summary>
public class GetOrganizationUnitTreeSummary : Summary<GetOrganizationUnitTreeEndpoint, GetOrganizationUnitTreeRequest>
{
    public GetOrganizationUnitTreeSummary()
    {
        Summary = "获取组织单位树形结构";
        Description = "查询组织单位的层级树形结构，支持选择是否包含非激活状态的组织单位";
        Response<IEnumerable<OrganizationUnitTreeDto>>(200, "成功获取组织单位树形结构");
        ExampleRequest = new GetOrganizationUnitTreeRequest(false);
        Responses[200] = "成功获取组织单位树形结构";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法查看组织单位";
    }
} 