using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Utils;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

/// <summary>
/// 密码重置的请求模型
/// </summary>
/// <param name="UserId">需要重置密码的用户ID</param>
public record PasswordResetRequest(UserId UserId);

/// <summary>
/// 密码重置的响应模型
/// </summary>
/// <param name="UserId">已重置密码的用户ID</param>
public record PasswordResetResponse(UserId UserId);

/// <summary>
/// 密码重置的API端点
/// 该端点用于重置指定用户的密码为默认密码（123456）
/// </summary>
/// <param name="mediator">中介者模式接口，用于处理命令和查询</param>
/// <param name="roleQuery">角色查询服务，用于执行角色相关的查询操作</param>
[Tags("Users")] // API文档标签，用于Swagger文档分组
public class PasswordResetEndpoint(IMediator mediator, RoleQuery roleQuery) : Endpoint<PasswordResetRequest, ResponseData<PasswordResetResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新用户密码
        Put("/api/user/password-reset");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // FastEndpoints 会自动从 JWT claims 中验证权限
        // 设置权限要求：用户必须同时拥有API访问权限和用户编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将用户密码重置为默认密码（123456）并返回结果
    /// </summary>
    /// <param name="request">包含用户ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(PasswordResetRequest request, CancellationToken ct)
    {
        // 对默认密码进行哈希处理
        // 注意：这里硬编码了默认密码"123456"，在生产环境中应该考虑更安全的密码策略
        var passwordHash = PasswordHasher.HashPassword("123456");
        
        // 创建密码重置命令对象
        var cmd = new PasswordResetCommand(request.UserId, passwordHash);
        
        // 通过中介者发送命令，执行实际的密码重置业务逻辑
        // 返回已重置密码的用户ID
        var userId = await mediator.Send(cmd, ct);
        
        // 创建响应对象，包含已重置密码的用户ID
        var response = new PasswordResetResponse(userId);
        
        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 密码重置端点的API文档配置
/// </summary>
public class PasswordResetSummary : Summary<PasswordResetEndpoint, PasswordResetRequest>
{
    public PasswordResetSummary()
    {
        Summary = "重置用户密码";
        Description = "将指定用户的密码重置为默认密码（123456），用户下次登录后应修改密码";
        Response<PasswordResetResponse>(200, "密码重置成功");
        ExampleRequest = new PasswordResetRequest(new UserId(1));
        Responses[200] = "成功重置用户密码";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足，无法重置密码";
        Responses[404] = "用户不存在";
        Responses[500] = "密码重置失败";
    }
}