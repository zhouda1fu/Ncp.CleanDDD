using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 删除组织单位的请求模型
/// </summary>
/// <param name="OrganizationUnitId">组织单位ID</param>
public record DeleteOrganizationUnitRequest(OrganizationUnitId OrganizationUnitId);

/// <summary>
/// 删除组织单位的API端点
/// 该端点用于从系统中删除指定的组织单位
/// </summary>
[Tags("OrganizationUnits")] // API文档标签，用于Swagger文档分组
public class DeleteOrganizationUnitEndpoint : Endpoint<DeleteOrganizationUnitRequest, ResponseData<bool>>
{
    /// <summary>
    /// 中介者模式接口，用于处理命令和查询
    /// </summary>
    private readonly IMediator _mediator;

    /// <summary>
    /// 构造函数，通过依赖注入获取中介者实例
    /// </summary>
    /// <param name="mediator">中介者接口实例</param>
    public DeleteOrganizationUnitEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP DELETE方法，用于删除组织单位
        Delete("/api/organization-units/{organizationUnitId}");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和组织单位删除权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitDelete);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 从路由获取组织单位ID，执行删除操作并返回结果
    /// </summary>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(DeleteOrganizationUnitRequest request, CancellationToken ct)
    {
        // 从路由参数中获取组织单位ID
        // 如果ID为空则抛出异常
        var organizationUnitId = Route<OrganizationUnitId>("organizationUnitId") ?? throw new KnownException("ID不能为空");
        
        // 创建删除命令对象
        var command = new DeleteOrganizationUnitCommand(organizationUnitId);
        
        // 通过中介者发送命令，执行实际的删除业务逻辑
        await _mediator.Send(command, ct);
        
        // 返回成功响应，表示删除操作完成
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 删除组织单位端点的API文档配置
/// </summary>
public class DeleteOrganizationUnitSummary : Summary<DeleteOrganizationUnitEndpoint>
{
    public DeleteOrganizationUnitSummary()
    {
        Summary = "删除组织单位";
        Description = "从系统中删除指定的组织单位，注意：删除前会检查是否有子组织单位或用户";
        Response<bool>(200, "组织单位删除成功");
        Responses[200] = "成功删除组织单位";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法删除组织单位";
        Responses[404] = "组织单位不存在";
        Responses[409] = "组织单位下还有子组织单位或用户，无法删除";
    }
} 