using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 从组织单位移除用户的请求模型
/// </summary>
/// <param name="UserId">要移除的用户ID</param>
public record RemoveUserOrganizationUnitRequest(UserId UserId);

/// <summary>
/// 从组织单位移除用户的响应模型
/// </summary>
/// <param name="UserId">已移除的用户ID</param>
public record RemoveUserOrganizationUnitResponse(UserId UserId);

/// <summary>
/// 从组织单位移除用户的API端点
/// 该端点用于将指定用户从当前所属的组织单位中移除
/// </summary>
/// <param name="mediator">中介者模式接口，用于处理命令和查询</param>
[Tags("OrganizationUnits")] // API文档标签，用于Swagger文档分组
public class RemoveUserOrganizationUnitEndpoint(IMediator mediator) : Endpoint<RemoveUserOrganizationUnitRequest, ResponseData<RemoveUserOrganizationUnitResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP DELETE方法，用于移除用户与组织单位的关联
        Delete("/api/organization-units/remove-user");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和组织单位管理权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitManagement);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回移除结果
    /// </summary>
    /// <param name="request">包含要移除用户ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(RemoveUserOrganizationUnitRequest request, CancellationToken ct)
    {
        // 创建移除用户组织单位命令对象
        var cmd = new RemoveUserOrganizationUnitCommand(request.UserId);
        
        // 通过中介者发送命令，执行实际的移除业务逻辑
        await mediator.Send(cmd, ct);
        
        // 创建响应对象，包含已移除的用户ID
        var response = new RemoveUserOrganizationUnitResponse(request.UserId);
        
        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 从组织单位移除用户端点的API文档配置
/// </summary>
public class RemoveUserOrganizationUnitSummary : Summary<RemoveUserOrganizationUnitEndpoint, RemoveUserOrganizationUnitRequest>
{
    public RemoveUserOrganizationUnitSummary()
    {
        Summary = "从组织单位移除用户";
        Description = "将指定用户从当前所属的组织单位中移除，解除用户与组织单位的关联关系";
        Response<RemoveUserOrganizationUnitResponse>(200, "用户移除成功");
        ExampleRequest = new RemoveUserOrganizationUnitRequest(new UserId(1));
        Responses[200] = "成功从组织单位移除用户";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法移除用户";
        Responses[404] = "用户不存在或未分配组织单位";
    }
} 