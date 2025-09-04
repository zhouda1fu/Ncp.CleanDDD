using FastEndpoints;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.Jwt;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.Configuration;
using System.Security.Claims;
using System.Text.Json;
using Ncp.CleanDDD.Web.Utils;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

/// <summary>
/// 刷新令牌的请求模型
/// </summary>
/// <param name="RefreshToken">需要刷新的刷新令牌</param>
public record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// 刷新令牌的响应模型
/// </summary>
/// <param name="Token">新的JWT访问令牌</param>
/// <param name="RefreshToken">新的刷新令牌</param>
/// <param name="UserId">用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">用户邮箱</param>
/// <param name="Permissions">用户权限（JSON格式）</param>
/// <param name="TokenExpiryTime">令牌过期时间</param>
public record RefreshTokenResponse(
    string Token, string RefreshToken, UserId UserId, string Name, string Email, string Permissions,
    DateTimeOffset TokenExpiryTime);

/// <summary>
/// 刷新令牌的API端点
/// 该端点用于刷新用户的访问令牌，延长会话有效期
/// </summary>
public class RefreshTokenEndpoint : Endpoint<RefreshTokenRequest, ResponseData<RefreshTokenResponse>>
{
    /// <summary>
    /// 中介者模式接口，用于处理命令和查询
    /// </summary>
    private readonly IMediator _mediator;
    
    /// <summary>
    /// 用户查询服务，用于执行用户相关的查询操作
    /// </summary>
    private readonly UserQuery _userQuery;
    
    /// <summary>
    /// JWT提供者，用于生成JWT令牌
    /// </summary>
    private readonly IJwtProvider _jwtProvider;
    
    /// <summary>
    /// 应用配置选项，包含令牌过期时间等设置
    /// </summary>
    private readonly IOptions<AppConfiguration> _appConfiguration;
    
    /// <summary>
    /// 角色查询服务，用于执行角色相关的查询操作
    /// </summary>
    private readonly RoleQuery _roleQuery;

    /// <summary>
    /// 构造函数，通过依赖注入获取所需的服务实例
    /// </summary>
    /// <param name="mediator">中介者接口实例</param>
    /// <param name="userQuery">用户查询服务实例</param>
    /// <param name="jwtProvider">JWT提供者实例</param>
    /// <param name="appConfiguration">应用配置选项</param>
    /// <param name="roleQuery">角色查询服务实例</param>
    public RefreshTokenEndpoint(IMediator mediator, UserQuery userQuery, IJwtProvider jwtProvider, IOptions<AppConfiguration> appConfiguration, RoleQuery roleQuery)
    {
        _mediator = mediator;
        _userQuery = userQuery;
        _jwtProvider = jwtProvider;
        _appConfiguration = appConfiguration;
        _roleQuery = roleQuery;
    }

    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、标签和访问权限等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于刷新令牌
        Post("/api/user/refresh-token");
        
        // 设置API文档标签
        Tags("Users");
        
        // 允许匿名访问，用户无需认证即可刷新令牌
        AllowAnonymous();
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 验证刷新令牌，生成新的访问令牌和刷新令牌，并更新用户登录信息
    /// </summary>
    /// <param name="req">包含刷新令牌的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        // 第一步：通过刷新令牌查询用户ID，验证令牌有效性
        var userId = await _userQuery.GetUserIdByRefreshTokenAsync(req.RefreshToken, ct) ?? throw new KnownException("无效的刷新令牌");

        // 第二步：根据用户ID获取用户登录信息
        var loginInfo = await _userQuery.GetUserInfoForLoginByIdAsync(userId, ct) ?? throw new KnownException("无效的用户");

        // 第三步：生成新的刷新令牌
        var refreshToken = TokenGenerator.GenerateRefreshToken();
        
        // 获取当前时间和计算令牌过期时间
        var nowTime = DateTime.Now;
        var tokenExpiryTime = nowTime.AddMinutes(_appConfiguration.Value.TokenExpiryInMinutes);
        
        // 获取用户的角色列表
        var roles = loginInfo.UserRoles.Select(r => r.RoleId).ToList();
        
        // 根据角色获取分配的权限代码
        var assignedPermissionCode = await _roleQuery.GetAssignedPermissionCodesAsync(roles, ct);
        
        // 创建JWT声明列表，包含用户基本信息
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

        // 使用 FastEndpoints 的 JWT 生成方式创建新的访问令牌
        var token = await _jwtProvider.GenerateJwtToken(new JwtData("issuer-x", "audience-y", claims, nowTime, tokenExpiryTime), ct);

        // 创建响应对象，包含新的令牌和用户信息
        var response = new RefreshTokenResponse(
            token,                                    // 新的访问令牌
            refreshToken,                             // 新的刷新令牌
            loginInfo.UserId,                         // 用户ID
            loginInfo.Name,                           // 用户名
            loginInfo.Email,                          // 用户邮箱
            JsonSerializer.Serialize(assignedPermissionCode), // 权限信息（JSON格式）
            tokenExpiryTime                           // 令牌过期时间
        );

        // 更新用户登录时间和刷新令牌
        var updateCmd = new UpdateUserLoginTimeCommand(loginInfo.UserId, DateTime.UtcNow, refreshToken);
        await _mediator.Send(updateCmd, ct);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 刷新令牌端点的API文档配置
/// </summary>
public class RefreshTokenSummary : Summary<RefreshTokenEndpoint, RefreshTokenRequest>
{
    public RefreshTokenSummary()
    {
        Summary = "刷新访问令牌";
        Description = "使用有效的刷新令牌获取新的访问令牌和刷新令牌，延长用户会话有效期";
        Response<RefreshTokenResponse>(200, "令牌刷新成功");
        ExampleRequest = new RefreshTokenRequest("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
        Responses[200] = "成功刷新令牌";
        Responses[400] = "请求参数无效";
        Responses[401] = "刷新令牌无效或已过期";
        Responses[404] = "用户不存在";
        Responses[500] = "令牌生成失败";
    }
}