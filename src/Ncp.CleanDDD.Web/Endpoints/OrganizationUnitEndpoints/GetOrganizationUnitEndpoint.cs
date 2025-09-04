using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 获取单个组织单位的响应模型
/// </summary>
/// <param name="Id">组织单位ID</param>
/// <param name="Name">组织单位名称</param>
/// <param name="Description">组织单位描述</param>
/// <param name="ParentId">父级组织单位ID</param>
/// <param name="SortOrder">排序顺序</param>
/// <param name="IsActive">是否激活</param>
/// <param name="CreatedAt">创建时间</param>
public record GetOrganizationUnitResponse(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId ParentId, int SortOrder, bool IsActive, DateTime CreatedAt);

/// <summary>
/// 获取单个组织单位的API端点
/// 该端点用于根据ID查询特定组织单位的详细信息
/// </summary>
[Tags("OrganizationUnits")] // API文档标签，用于Swagger文档分组
public class GetOrganizationUnitEndpoint : EndpointWithoutRequest<ResponseData<GetOrganizationUnitResponse?>>
{
    /// <summary>
    /// 组织单位查询服务，用于执行组织单位相关的查询操作
    /// </summary>
    private readonly OrganizationUnitQuery _organizationUnitQuery;

    /// <summary>
    /// 构造函数，通过依赖注入获取组织单位查询服务实例
    /// </summary>
    /// <param name="organizationUnitQuery">组织单位查询服务实例</param>
    public GetOrganizationUnitEndpoint(OrganizationUnitQuery organizationUnitQuery)
    {
        _organizationUnitQuery = organizationUnitQuery;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，通过路由参数获取组织单位ID
        Get("/api/organization-units/{organizationUnitId}");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和组织单位查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 从路由中获取组织单位ID，查询详细信息并返回结果
    /// </summary>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        // 从路由参数中获取组织单位ID
        var organizationUnitId = Route<long>("organizationUnitId");
        
        // 通过查询服务获取组织单位详细信息
        // 如果不存在则抛出已知异常
        var organizationUnit = await _organizationUnitQuery.GetOrganizationUnitByIdAsync(new OrganizationUnitId(organizationUnitId), ct) ?? 
                               throw new KnownException("组织架构不存在");
        
        // 创建响应对象，包含组织单位的详细信息
        var response = new GetOrganizationUnitResponse(
            organizationUnit.Id,           // 组织单位ID
            organizationUnit.Name,         // 组织单位名称
            organizationUnit.Description,  // 组织单位描述
            organizationUnit.ParentId,     // 父级组织单位ID
            organizationUnit.SortOrder,    // 排序顺序
            organizationUnit.IsActive,     // 是否激活
            organizationUnit.CreatedAt     // 创建时间
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 获取单个组织单位端点的API文档配置
/// </summary>
public class GetOrganizationUnitSummary : Summary<GetOrganizationUnitEndpoint>
{
    public GetOrganizationUnitSummary()
    {
        Summary = "获取组织单位详情";
        Description = "根据组织单位ID查询特定组织单位的详细信息";
        Response<GetOrganizationUnitResponse>(200, "成功获取组织单位详情");
        Responses[200] = "成功获取组织单位详情";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法查看组织单位";
        Responses[404] = "组织单位不存在";
    }
} 