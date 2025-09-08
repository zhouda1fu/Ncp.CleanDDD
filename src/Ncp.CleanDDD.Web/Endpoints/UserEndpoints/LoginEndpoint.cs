using FastEndpoints;
using Microsoft.Extensions.Options;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Configuration;
using Ncp.CleanDDD.Web.Utils;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

/// <summary>
/// 用户登录的请求模型
/// </summary>
/// <param name="Username">用户名</param>
/// <param name="Password">密码</param>
public record LoginRequest(string Username, string Password);

/// <summary>
/// 用户登录的响应模型
/// </summary>
/// <param name="Token">JWT访问令牌</param>
/// <param name="RefreshToken">刷新令牌</param>
/// <param name="UserId">用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">用户邮箱</param>
/// <param name="Permissions">用户权限（JSON格式）</param>
/// <param name="TokenExpiryTime">令牌过期时间</param>
public record LoginResponse(string Token, string RefreshToken, UserId UserId, string Name, string Email, string Permissions,
    DateTimeOffset TokenExpiryTime);

/// <summary>
/// 用户登录的API端点
/// 该端点用于验证用户凭据，生成JWT令牌和刷新令牌，并更新用户登录信息
/// </summary>
/// <param name="mediator">中介者模式接口，用于处理命令和查询</param>
/// <param name="userQuery">用户查询服务，用于执行用户相关的查询操作</param>
/// <param name="jwtProvider">JWT提供者，用于生成JWT令牌</param>
/// <param name="appConfiguration">应用配置选项，包含令牌过期时间等设置</param>
/// <param name="roleQuery">角色查询服务，用于执行角色相关的查询操作</param>
public class LoginEndpoint(IMediator mediator, UserQuery userQuery, IJwtProvider jwtProvider, IOptions<AppConfiguration> appConfiguration, RoleQuery roleQuery) : Endpoint<LoginRequest, ResponseData<LoginResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、标签和访问权限等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于用户登录
        Post("/api/user/login");
        
        // 设置API文档标签
        Tags("Users");
        
        // 允许匿名访问，用户无需认证即可登录
        AllowAnonymous();
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 验证用户凭据，生成JWT令牌和刷新令牌，并更新用户登录信息
    /// </summary>
    /// <param name="req">包含用户名和密码的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        // 第一步：查询并验证用户凭据
        var loginInfo = await userQuery.GetUserInfoForLoginAsync(req.Username, ct) ?? throw new KnownException("无效的用户");
        
        // 验证密码哈希是否匹配
        if (!PasswordHasher.VerifyHashedPassword(req.Password, loginInfo.PasswordHash))
            throw new KnownException("用户名或密码错误");

        // 第二步：生成JWT令牌和刷新令牌
        var refreshToken = TokenGenerator.GenerateRefreshToken();
        var nowTime = DateTimeOffset.Now;
        var tokenExpiryTime = nowTime.AddMinutes(appConfiguration.Value.TokenExpiryInMinutes);
        
        // 获取用户的角色列表
        var roles = loginInfo.UserRoles.Select(r => r.RoleId).ToList();
        
        // 根据角色获取分配的权限代码
        var assignedPermissionCode = await roleQuery.GetAssignedPermissionCodesAsync(roles, ct);
        
        // 创建JWT声明列表，包含用户基本信息和权限
        var claims = new List<Claim>
        {
            new("name", loginInfo.Name),           // 用户名
            new("email", loginInfo.Email),         // 用户邮箱
            new("sub", loginInfo.UserId.ToString()), // 用户主题标识
            new("user_id", loginInfo.UserId.ToString()) // 用户ID
        };

        // 添加权限到声明中，FastEndpoints 会自动处理权限验证
        if (assignedPermissionCode != null)
        {
            foreach (var permissionCode in assignedPermissionCode)
            {
                claims.Add(new Claim("permissions", permissionCode));
            }
        }

        // 使用 FastEndpoints 的 JWT 生成方式创建访问令牌
        var token = await jwtProvider.GenerateJwtToken(new JwtData("issuer-x", "audience-y", claims, nowTime.UtcDateTime, tokenExpiryTime.UtcDateTime), ct);

        // 创建登录响应对象，包含令牌和用户信息
        var response = new LoginResponse(
            token,                                    // JWT访问令牌
            refreshToken,                             // 刷新令牌
            loginInfo.UserId,                         // 用户ID
            loginInfo.Name,                           // 用户名
            loginInfo.Email,                          // 用户邮箱
            JsonSerializer.Serialize(assignedPermissionCode), // 权限信息（JSON格式）
            tokenExpiryTime                           // 令牌过期时间
        );

        // 更新用户登录时间和刷新令牌
        var updateCmd = new UpdateUserLoginTimeCommand(loginInfo.UserId, DateTimeOffset.UtcNow, refreshToken);
        await mediator.Send(updateCmd, ct);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 用户登录端点的API文档配置
/// </summary>
public class LoginSummary : Summary<LoginEndpoint, LoginRequest>
{
    public LoginSummary()
    {
        Summary = "用户登录";
        Description = "验证用户凭据，生成JWT访问令牌和刷新令牌，支持权限验证和会话管理";
        Response<LoginResponse>(200, "登录成功");
        ExampleRequest = new LoginRequest("john.doe", "SecurePassword123");
        Responses[200] = "成功登录并获取令牌";
        Responses[400] = "请求参数无效";
        Responses[401] = "用户名或密码错误";
        Responses[404] = "用户不存在";
        Responses[500] = "登录处理失败";
    }
}