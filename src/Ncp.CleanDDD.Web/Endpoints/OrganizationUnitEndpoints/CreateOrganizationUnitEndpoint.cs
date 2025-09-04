using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 创建组织单位的请求模型
/// </summary>
/// <param name="Name">组织单位名称</param>
/// <param name="Description">组织单位描述</param>
/// <param name="ParentId">父级组织单位ID，可为空表示顶级组织</param>
/// <param name="SortOrder">排序顺序</param>
public record CreateOrganizationUnitRequest(string Name, string Description, OrganizationUnitId? ParentId, int SortOrder);

/// <summary>
/// 创建组织单位的响应模型
/// </summary>
/// <param name="Id">新创建的组织单位ID</param>
/// <param name="Name">组织单位名称</param>
/// <param name="Description">组织单位描述</param>
public record CreateOrganizationUnitResponse(OrganizationUnitId Id, string Name, string Description);

/// <summary>
/// 创建组织单位的API端点
/// 该端点用于在系统中创建新的组织单位，支持层级结构
/// </summary>
[Tags("OrganizationUnits")] // API文档标签，用于Swagger文档分组
public class CreateOrganizationUnitEndpoint : Endpoint<CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>
{
    /// <summary>
    /// 中介者模式接口，用于处理命令和查询
    /// </summary>
    private readonly IMediator _mediator;

    /// <summary>
    /// 构造函数，通过依赖注入获取中介者实例
    /// </summary>
    /// <param name="mediator">中介者接口实例</param>
    public CreateOrganizationUnitEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于创建新的组织单位
        Post("/api/organization-units");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和组织单位创建权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitCreate);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回新创建的组织单位信息
    /// </summary>
    /// <param name="req">包含组织单位基本信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(CreateOrganizationUnitRequest req, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        // 如果父级ID为空，则设置为根组织单位（ID为0）
        var command = new CreateOrganizationUnitCommand(
            req.Name,           // 组织单位名称
            req.Description,    // 组织单位描述
            req.ParentId ?? new OrganizationUnitId(0), // 父级组织单位ID，默认为根组织
            req.SortOrder       // 排序顺序
        );

        // 通过中介者发送命令，执行实际的业务逻辑
        // 返回新创建的组织单位ID
        var organizationUnitId = await _mediator.Send(command, ct);

        // 创建响应对象，包含新创建的组织单位信息
        var response = new CreateOrganizationUnitResponse(
            organizationUnitId, // 新创建的组织单位ID
            req.Name,           // 组织单位名称
            req.Description     // 组织单位描述
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建组织单位端点的API文档配置
/// </summary>
public class CreateOrganizationUnitSummary : Summary<CreateOrganizationUnitEndpoint, CreateOrganizationUnitRequest>
{
    public CreateOrganizationUnitSummary()
    {
        Summary = "创建组织单位";
        Description = "在系统中创建新的组织单位，支持层级结构管理";
        Response<CreateOrganizationUnitResponse>(200, "组织单位创建成功");
        ExampleRequest = new CreateOrganizationUnitRequest(
            "技术部", 
            "负责技术研发工作", 
            null, 
            1
        );
        Responses[200] = "成功创建组织单位";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法创建组织单位";
        Responses[409] = "组织单位名称已存在";
    }
}