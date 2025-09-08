using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 分配用户到组织单位的请求模型
/// </summary>
/// <param name="UserId">用户ID</param>
/// <param name="OrganizationUnitId">组织单位ID</param>
/// <param name="OrganizationUnitName">组织单位名称</param>
public record AssignUserOrganizationUnitRequest(UserId UserId, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName);

/// <summary>
/// 分配用户到组织单位的响应模型
/// </summary>
/// <param name="UserId">已分配的用户ID</param>
public record AssignUserOrganizationUnitResponse(UserId UserId);

/// <summary>
/// 分配用户到组织单位的API端点
/// 该端点用于将指定用户分配到指定的组织单位中
/// </summary>
/// <param name="mediator">中介者模式接口，用于处理命令和查询</param>
[Tags("OrganizationUnits")] // API文档标签，用于Swagger文档分组
public class AssignUserOrganizationUnitEndpoint(IMediator mediator) : Endpoint<AssignUserOrganizationUnitRequest, ResponseData<AssignUserOrganizationUnitResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于创建或分配资源
        Post("/api/organization-units/assign-user");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和组织单位编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回响应
    /// </summary>
    /// <param name="request">包含用户ID、组织单位ID和组织单位名称的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(AssignUserOrganizationUnitRequest request, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        // 这里使用了命令模式，将业务逻辑封装在命令中
        var command = new AssignUserOrganizationUnitCommand(
            request.UserId,           // 要分配的用户ID
            request.OrganizationUnitId, // 目标组织单位ID
            request.OrganizationUnitName // 组织单位名称（用于日志记录或验证）
        );

        // 通过中介者发送命令，执行实际的业务逻辑
        // 中介者会将命令路由到相应的命令处理器
        await mediator.Send(command, ct);

        // 创建响应对象，包含已分配的用户ID
        var response = new AssignUserOrganizationUnitResponse(request.UserId);
        
        // 返回成功响应，使用统一的响应数据格式包装
        // AsResponseData()方法将响应包装在标准的API响应结构中
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 分配用户到组织单位端点的API文档配置
/// </summary>
public class AssignUserOrganizationUnitSummary : Summary<AssignUserOrganizationUnitEndpoint, AssignUserOrganizationUnitRequest>
{
    public AssignUserOrganizationUnitSummary()
    {
        Summary = "分配用户到组织单位";
        Description = "将指定用户分配到指定的组织单位中，支持用户组织架构管理";
        Response<AssignUserOrganizationUnitResponse>(200, "用户分配成功");
        ExampleRequest = new AssignUserOrganizationUnitRequest(
            new UserId(1), 
            new OrganizationUnitId(1), 
            "技术部"
        );
        Responses[200] = "成功分配用户到组织单位";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法执行此操作";
        Responses[404] = "用户或组织单位不存在";
        Responses[409] = "用户已在该组织单位中";
    }
} 