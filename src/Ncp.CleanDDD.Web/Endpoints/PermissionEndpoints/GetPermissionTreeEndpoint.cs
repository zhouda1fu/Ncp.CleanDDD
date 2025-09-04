using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.PermissionEndpoints;

/// <summary>
/// 获取权限树形结构的API端点
/// 该端点用于查询系统中所有权限的分组树形结构，便于前端展示权限管理界面
/// </summary>
[Tags("Permissions")] // API文档标签，用于Swagger文档分组
public class GetPermissionTreeEndpoint : EndpointWithoutRequest<ResponseData<IEnumerable<AppPermissionGroup>>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询权限树形结构
        Get("/api/permissions/tree");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和用户角色分配权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserRoleAssign);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 从权限定义上下文中获取权限分组信息并返回结果
    /// </summary>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        // 从权限定义上下文中获取所有权限分组信息
        // 这些分组信息在应用启动时就已经配置好
        var permissionGroups = PermissionDefinitionContext.PermissionGroups;
        
        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(new ResponseData<IEnumerable<AppPermissionGroup>>(permissionGroups), cancellation: ct);
    }
}

/// <summary>
/// 获取权限树形结构端点的API文档配置
/// </summary>
public class GetPermissionTreeSummary : Summary<GetPermissionTreeEndpoint>
{
    public GetPermissionTreeSummary()
    {
        Summary = "获取权限树形结构";
        Description = "查询系统中所有权限的分组树形结构，用于权限管理和角色分配";
        Response<IEnumerable<AppPermissionGroup>>(200, "成功获取权限树形结构");
        Responses[200] = "成功获取权限树形结构";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法查看权限信息";
    }
} 