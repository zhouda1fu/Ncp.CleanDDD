using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Web.Application.Commands.RoleCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 更新角色信息的请求模型
/// </summary>
/// <param name="RoleId">要更新的角色ID</param>
/// <param name="Name">新的角色名称</param>
/// <param name="Description">新的角色描述</param>
/// <param name="PermissionCodes">新的权限代码列表</param>
public record UpdateRoleInfoRequest(RoleId RoleId, string Name, string Description, IEnumerable<string> PermissionCodes);

/// <summary>
/// 更新角色信息的API端点
/// 该端点用于修改现有角色的基本信息和权限分配
/// </summary>
[Tags("Roles")] // API文档标签，用于Swagger文档分组
public class UpdateRoleEndpoint : Endpoint<UpdateRoleInfoRequest, ResponseData<bool>>
{
    /// <summary>
    /// 中介者模式接口，用于处理命令和查询
    /// </summary>
    private readonly IMediator _mediator;

    /// <summary>
    /// 构造函数，通过依赖注入获取中介者实例
    /// </summary>
    /// <param name="roleQuery">角色查询服务实例（当前未使用）</param>
    /// <param name="mediator">中介者接口实例</param>
    public UpdateRoleEndpoint(RoleQuery roleQuery, IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新角色信息
        Put("/api/roles/update");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和角色编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回更新结果
    /// </summary>
    /// <param name="request">包含角色更新信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UpdateRoleInfoRequest request, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        var cmd = new UpdateRoleInfoCommand(
            request.RoleId,           // 要更新的角色ID
            request.Name,             // 新的角色名称
            request.Description,      // 新的角色描述
            request.PermissionCodes   // 新的权限代码列表
        );
        
        // 通过中介者发送命令，执行实际的更新业务逻辑
        await _mediator.Send(cmd, ct);
        
        // 返回成功响应，表示更新操作完成
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 更新角色信息端点的API文档配置
/// </summary>
public class UpdateRoleSummary : Summary<UpdateRoleEndpoint, UpdateRoleInfoRequest>
{
    public UpdateRoleSummary()
    {
        Summary = "更新角色信息";
        Description = "修改现有角色的基本信息，包括名称、描述和权限分配";
        Response<bool>(200, "角色信息更新成功");
        ExampleRequest = new UpdateRoleInfoRequest(
            new RoleId(Guid.NewGuid()), 
            "高级管理员", 
            "拥有系统管理权限的高级角色", 
            new[] { "UserView", "UserEdit", "RoleView", "RoleEdit", "SystemConfig" }
        );
        Responses[200] = "成功更新角色信息";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法更新角色";
        Responses[404] = "角色不存在";
        Responses[409] = "角色名称已存在";
    }
}