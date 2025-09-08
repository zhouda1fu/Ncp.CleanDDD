using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;


public record DeleteRoleRequest(RoleId RoleId);

/// <summary>
/// 删除角色的API端点
/// 该端点用于从系统中删除指定的角色
/// </summary>
/// <param name="mediator">中介者模式接口，用于处理命令和查询</param>
[Tags("Roles")] // API文档标签，用于Swagger文档分组
public class DeleteRoleEndpoint(IMediator mediator) : Endpoint<DeleteRoleRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP DELETE方法，用于删除角色
        Delete("/api/roles/{roleId}");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和角色删除权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleDelete);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 从路由获取角色ID，执行删除操作并返回结果
    /// </summary>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(DeleteRoleRequest request, CancellationToken ct)
    {
        // 从路由参数中获取角色ID
        // 如果ID为空则抛出异常
        var roleId = Route<RoleId>("roleId") ?? throw new KnownException("ID不能为空");
        
        // 创建删除角色命令对象
        var command = new DeleteRoleCommand(roleId);
        
        // 通过中介者发送命令，执行实际的删除业务逻辑
        await mediator.Send(command, ct);
        
        // 返回成功响应，表示删除操作完成
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 删除角色端点的API文档配置
/// </summary>
public class DeleteRoleSummary : Summary<DeleteRoleEndpoint>
{
    public DeleteRoleSummary()
    {
        Summary = "删除角色";
        Description = "从系统中删除指定的角色，注意：删除前会检查是否有用户正在使用该角色";
        Response<bool>(200, "角色删除成功");
        Responses[200] = "成功删除角色";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法删除角色";
        Responses[404] = "角色不存在";
        Responses[409] = "角色正在被用户使用，无法删除";
    }
}