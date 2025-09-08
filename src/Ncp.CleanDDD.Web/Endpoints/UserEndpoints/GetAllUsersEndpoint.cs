using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

/// <summary>
/// 获取所有用户信息的API端点
/// 该端点用于查询系统中的所有用户信息，支持分页、筛选和搜索
/// </summary>
public class GetAllUsersEndpoint(UserQuery userQuery) : Endpoint<UserQueryInput, ResponseData<PagedData<UserInfoQueryDto>>>
{
   

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询用户信息
        Get("/api/users");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 注释掉的匿名访问设置，当前要求认证
        //AllowAnonymous();
        
        // 设置权限要求：用户必须同时拥有API访问权限和用户查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
        
        // 设置API文档标签
        Tags("Users");
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 查询所有用户信息并返回分页结果
    /// </summary>
    /// <param name="req">用户查询输入参数，包含分页和筛选条件</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UserQueryInput req, CancellationToken ct)
    {
        // 通过查询服务获取所有用户信息，支持分页和筛选
        var result = await userQuery.GetAllUsersAsync(req, ct);
        
        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }

    /// <summary>
    /// 获取所有用户端点的API文档配置
    /// </summary>
    public class GetAllUsersSummary : Summary<GetAllUsersEndpoint, UserQueryInput>
    {
        public GetAllUsersSummary()
        {
            Summary = "获取所有用户";
            Description = "查询系统中的所有用户信息，支持分页、筛选、搜索和排序";
            Response<PagedData<UserInfoQueryDto>>(200, "成功获取用户列表");
            Responses[200] = "成功获取用户列表";
            Responses[400] = "请求参数无效";
            Responses[401] = "未授权访问";
            Responses[403] = "权限不足，无法查看用户";
            Responses[500] = "查询失败";
        }
    }
}