using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 更新组织单位的请求模型
/// </summary>
/// <param name="Id">OrganizationUnitId</param>
/// <param name="Name">组织单位名称</param>
/// <param name="Description">组织单位描述</param>
/// <param name="ParentId">父级组织单位ID，可为空表示顶级组织</param>
/// <param name="SortOrder">排序顺序</param>
public record UpdateOrganizationUnitRequest(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId? ParentId, int SortOrder);

/// <summary>
/// 更新组织单位的API端点
/// 该端点用于修改现有组织单位的基本信息
/// </summary>
/// <param name="mediator">中介者模式接口，用于处理命令和查询</param>
[Tags("OrganizationUnits")] // API文档标签，用于Swagger文档分组
public class UpdateOrganizationUnitEndpoint(IMediator mediator) : Endpoint<UpdateOrganizationUnitRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新组织单位信息
        Put("/api/organization-units");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和组织单位编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 从路由获取组织单位ID，将请求转换为命令并执行更新操作
    /// </summary>
    /// <param name="request">包含更新信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UpdateOrganizationUnitRequest request, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        // 如果父级ID为空，则设置为根组织单位（ID为0）
        var command = new UpdateOrganizationUnitCommand(
            request.Id,                    // 要更新的组织单位ID
            request.Name,                          // 新的组织单位名称
            request.Description,                   // 新的组织单位描述
            request.ParentId ?? new OrganizationUnitId(0), // 父级组织单位ID，默认为根组织
            request.SortOrder                      // 新的排序顺序
        );

        // 通过中介者发送命令，执行实际的更新业务逻辑
        await mediator.Send(command, ct);
        
        // 返回成功响应，表示更新操作完成
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 更新组织单位端点的API文档配置
/// </summary>
public class UpdateOrganizationUnitSummary : Summary<UpdateOrganizationUnitEndpoint, UpdateOrganizationUnitRequest>
{
    public UpdateOrganizationUnitSummary()
    {
        Summary = "更新组织单位";
        Description = "修改现有组织单位的基本信息，包括名称、描述、父级组织和排序";
        Response<bool>(200, "组织单位更新成功");
        ExampleRequest = new UpdateOrganizationUnitRequest(
            new OrganizationUnitId(1),
            "技术研发部", 
            "负责技术研发和产品开发", 
            null, 
            1
        );
        Responses[200] = "成功更新组织单位";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法更新组织单位";
        Responses[404] = "组织单位不存在";
        Responses[409] = "组织单位名称已存在";
    }
}