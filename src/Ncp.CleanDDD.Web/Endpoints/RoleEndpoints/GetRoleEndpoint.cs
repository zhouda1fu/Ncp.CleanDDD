using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 获取角色信息的请求模型
/// </summary>
/// <param name="RoleId">要查询的角色ID</param>
public record GetRoleRequest(RoleId RoleId);

/// <summary>
/// 获取角色信息的响应模型
/// </summary>
/// <param name="Id">角色ID</param>
/// <param name="Name">角色名称</param>
/// <param name="Description">角色描述</param>
/// <param name="IsActive">是否激活</param>
/// <param name="CreatedAt">创建时间</param>
public record GetRoleResponse(RoleId Id, string Name, string Description, bool IsActive, DateTime CreatedAt);

/// <summary>
/// 获取角色信息的API端点
/// 该端点用于根据ID查询特定角色的详细信息
/// </summary>
[Tags("Roles")] // API文档标签，用于Swagger文档分组
public class GetRoleEndpoint : Endpoint<GetRoleRequest, ResponseData<GetRoleResponse?>>
{
    /// <summary>
    /// 角色查询服务，用于执行角色相关的查询操作
    /// </summary>
    private readonly RoleQuery _roleQuery;

    /// <summary>
    /// 构造函数，通过依赖注入获取角色查询服务实例
    /// </summary>
    /// <param name="roleQuery">角色查询服务实例</param>
    public GetRoleEndpoint(RoleQuery roleQuery)
    {
        _roleQuery = roleQuery;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，通过路由参数获取角色ID
        Get("/api/roles/{roleId}");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和角色查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 根据角色ID查询角色详细信息并返回结果
    /// </summary>
    /// <param name="req">包含角色ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetRoleRequest req, CancellationToken ct)
    {
        // 通过查询服务获取角色详细信息
        // 如果角色不存在则抛出已知异常
        var roleInfo = await _roleQuery.GetRoleByIdAsync(req.RoleId, ct) ?? throw new KnownException("Invalid Credentials.");
        
        // 创建响应对象，包含角色的详细信息
        var response = new GetRoleResponse(
            roleInfo.RoleId,      // 角色ID
            roleInfo.Name,        // 角色名称
            roleInfo.Description, // 角色描述
            roleInfo.IsActive,    // 是否激活
            roleInfo.CreatedAt    // 创建时间
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 获取角色信息端点的API文档配置
/// </summary>
public class GetRoleSummary : Summary<GetRoleEndpoint, GetRoleRequest>
{
    public GetRoleSummary()
    {
        Summary = "获取角色信息";
        Description = "根据角色ID查询特定角色的详细信息，包括基本信息、状态和创建时间";
        Response<GetRoleResponse>(200, "成功获取角色信息");
        ExampleRequest = new GetRoleRequest(new RoleId(Guid.NewGuid()));
        Responses[200] = "成功获取角色信息";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法查看角色";
        Responses[404] = "角色不存在";
        Responses[500] = "查询失败，凭据无效";
    }
} 