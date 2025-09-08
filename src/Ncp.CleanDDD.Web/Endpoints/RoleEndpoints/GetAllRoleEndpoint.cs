using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 获取所有角色的API端点
/// 该端点用于查询系统中的所有角色信息，支持分页和筛选
/// </summary>
[Tags("Roles")] // API文档标签，用于Swagger文档分组
public class GetAllRoleEndpoint(RoleQuery roleQuery) : Endpoint<RoleQueryInput, ResponseData<PagedData<RoleQueryDto>?>>
{
  

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询角色信息
        Get("/api/roles");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和角色查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
        
        // 注释掉的匿名访问设置，当前要求认证
        //AllowAnonymous();
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 查询所有角色信息并返回分页结果
    /// </summary>
    /// <param name="req">角色查询输入参数，包含分页和筛选条件</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(RoleQueryInput req, CancellationToken ct)
    {
        // 通过查询服务获取所有角色信息
        // 如果查询失败则抛出已知异常
        var roleInfo = await roleQuery.GetAllRolesAsync(req, ct) ?? throw new KnownException("Invalid Credentials.");
        
        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(roleInfo.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 获取所有角色端点的API文档配置
/// </summary>
public class GetAllRoleSummary : Summary<GetAllRoleEndpoint, RoleQueryInput>
{
    public GetAllRoleSummary()
    {
        Summary = "获取所有角色";
        Description = "查询系统中的所有角色信息，支持分页、筛选和搜索";
        Response<PagedData<RoleQueryDto>>(200, "成功获取角色列表");
        Responses[200] = "成功获取角色列表";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法查看角色";
        Responses[500] = "查询失败，凭据无效";
    }
} 