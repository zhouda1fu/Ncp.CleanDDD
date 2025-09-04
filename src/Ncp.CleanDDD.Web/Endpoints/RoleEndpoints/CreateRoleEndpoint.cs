using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Web.Application.Commands;
using Ncp.CleanDDD.Web.Application.Commands.RoleCommands;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 创建角色的请求模型
/// </summary>
/// <param name="Name">角色名称</param>
/// <param name="Description">角色描述</param>
/// <param name="PermissionCodes">权限代码列表</param>
public record CreateRoleRequest(string Name, string Description, IEnumerable<string> PermissionCodes);

/// <summary>
/// 创建角色的响应模型
/// </summary>
/// <param name="RoleId">新创建的角色ID</param>
/// <param name="Name">角色名称</param>
/// <param name="Description">角色描述</param>
public record CreateRoleResponse(string RoleId, string Name, string Description);

/// <summary>
/// 创建角色的API端点
/// 该端点用于在系统中创建新的角色，并分配相应的权限
/// </summary>
[Tags("Roles")] // API文档标签，用于Swagger文档分组
public class CreateRoleEndpoint : Endpoint<CreateRoleRequest, ResponseData<CreateRoleResponse>>
{
    /// <summary>
    /// 中介者模式接口，用于处理命令和查询
    /// </summary>
    private readonly IMediator _mediator;

    /// <summary>
    /// 构造函数，通过依赖注入获取中介者实例
    /// </summary>
    /// <param name="mediator">中介者接口实例</param>
    public CreateRoleEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于创建新的角色
        Post("/api/roles");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和角色创建权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleCreate);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回新创建的角色信息
    /// </summary>
    /// <param name="req">包含角色基本信息和权限的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        var cmd = new CreateRoleCommand(req.Name, req.Description, req.PermissionCodes);
        
        // 通过中介者发送命令，执行实际的业务逻辑
        // 返回新创建的角色ID
        var result = await _mediator.Send(cmd, ct);
        
        // 创建响应对象，包含新创建的角色信息
        var response = new CreateRoleResponse(
            result.ToString(),    // 新创建的角色ID
            req.Name,             // 角色名称
            req.Description       // 角色描述
        );
        
        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建角色端点的API文档配置
/// </summary>
public class CreateRoleSummary : Summary<CreateRoleEndpoint, CreateRoleRequest>
{
    public CreateRoleSummary()
    {
        Summary = "创建角色";
        Description = "在系统中创建新的角色，并分配相应的权限代码";
        Response<CreateRoleResponse>(200, "角色创建成功");
        ExampleRequest = new CreateRoleRequest(
            "管理员", 
            "系统管理员角色，拥有所有权限", 
            new[] { "UserView", "UserEdit", "RoleView", "RoleEdit" }
        );
        Responses[200] = "成功创建角色";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法创建角色";
        Responses[409] = "角色名称已存在";
    }
} 