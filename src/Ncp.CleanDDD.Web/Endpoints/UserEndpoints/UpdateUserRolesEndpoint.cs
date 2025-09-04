using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

/// <summary>
/// 更新用户角色的请求模型
/// </summary>
/// <param name="UserId">要更新角色的用户ID</param>
/// <param name="RoleIds">要分配给用户的角色ID列表</param>
public record UpdateUserRolesRequest(UserId UserId, IEnumerable<RoleId> RoleIds);

/// <summary>
/// 更新用户角色的响应模型
/// </summary>
/// <param name="UserId">已更新角色的用户ID</param>
public record UpdateUserRolesResponse(UserId UserId);

/// <summary>
/// 更新用户角色的API端点
/// 该端点用于修改指定用户的角色分配，支持批量角色分配
/// </summary>
[Tags("Users")] // API文档标签，用于Swagger文档分组
public class UpdateUserRolesEndpoint : Endpoint<UpdateUserRolesRequest, ResponseData<UpdateUserRolesResponse>>
{
    /// <summary>
    /// 中介者模式接口，用于处理命令和查询
    /// </summary>
    private readonly IMediator _mediator;
    
    /// <summary>
    /// 角色查询服务，用于执行角色相关的查询操作
    /// </summary>
    private readonly RoleQuery _roleQuery;

    /// <summary>
    /// 构造函数，通过依赖注入获取中介者和角色查询服务实例
    /// </summary>
    /// <param name="mediator">中介者接口实例</param>
    /// <param name="roleQuery">角色查询服务实例</param>
    public UpdateUserRolesEndpoint(IMediator mediator, RoleQuery roleQuery)
    {
        _mediator = mediator;
        _roleQuery = roleQuery;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新用户角色分配
        Put("/api/users/update-roles");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和用户角色分配权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserRoleAssign);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 验证角色信息，更新用户角色分配并返回结果
    /// </summary>
    /// <param name="request">包含用户ID和角色ID列表的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UpdateUserRolesRequest request, CancellationToken ct)
    {
        // 通过角色查询服务验证要分配的角色信息
        // 确保角色存在且可用于分配
        var rolesToBeAssigned = await _roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);

        // 创建更新用户角色命令对象
        var cmd = new UpdateUserRolesCommand(request.UserId, rolesToBeAssigned);
        
        // 通过中介者发送命令，执行实际的角色分配业务逻辑
        await _mediator.Send(cmd, ct);
        
        // 创建响应对象，包含已更新角色的用户ID
        var response = new UpdateUserRolesResponse(request.UserId);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 更新用户角色端点的API文档配置
/// </summary>
public class UpdateUserRolesSummary : Summary<UpdateUserRolesEndpoint, UpdateUserRolesRequest>
{
    public UpdateUserRolesSummary()
    {
        Summary = "更新用户角色";
        Description = "修改指定用户的角色分配，支持批量角色分配和角色验证";
        Response<UpdateUserRolesResponse>(200, "用户角色更新成功");
        ExampleRequest = new UpdateUserRolesRequest(
            new UserId(1), 
            new[] { new RoleId(Guid.NewGuid()), new RoleId(Guid.NewGuid()) }
        );
        Responses[200] = "成功更新用户角色";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法分配角色";
        Responses[404] = "用户或角色不存在";
        Responses[409] = "角色分配冲突";
    }
}