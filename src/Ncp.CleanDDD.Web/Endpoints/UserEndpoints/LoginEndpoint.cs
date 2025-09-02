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
/// 用户名和密码登录请求DTO
/// </summary>
/// <param name="Username"></param>
/// <param name="Password"></param>
public record LoginRequest(string Username, string Password);

/// <summary>
/// 
/// </summary>
/// <param name="Token"></param>
/// <param name="RefreshToken"></param>
/// <param name="UserId"></param>
/// <param name="Name"></param>
/// <param name="Email"></param>
/// <param name="Permissions"></param>
/// <param name="TokenExpiryTime"></param>
public record LoginResponse(string Token,string RefreshToken, UserId UserId, string Name, string Email,string Permissions,
    DateTimeOffset TokenExpiryTime);


/// <summary>
/// 登录
/// </summary>
public class LoginEndpoint : Endpoint<LoginRequest, ResponseData<LoginResponse>>
{
    private readonly IMediator _mediator;
    private readonly UserQuery _userQuery;
    private readonly IJwtProvider _jwtProvider;
    private readonly IOptions<AppConfiguration> _appConfiguration;
    private readonly RoleQuery _roleQuery;

    public LoginEndpoint(IMediator mediator, UserQuery userQuery, IJwtProvider jwtProvider, IOptions<AppConfiguration> appConfiguration, RoleQuery roleQuery)
    {
        _mediator = mediator;
        _userQuery = userQuery;
        _jwtProvider = jwtProvider;
        _appConfiguration = appConfiguration;
        _roleQuery = roleQuery;
    }

    public override void Configure()
    {
        Post("/api/user/login");
        Tags("Users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {

        // 1. 查询：验证用户凭据
        var loginInfo = await _userQuery.GetUserInfoForLoginAsync(req.Username, ct) ?? throw new KnownException("无效的用户");
        if (!PasswordHasher.VerifyHashedPassword(req.Password, loginInfo.PasswordHash))
            throw new KnownException("用户名或密码错误");


        //  生成JWT令牌

        var refreshToken = TokenGenerator.GenerateRefreshToken();
        var nowTime = DateTime.Now;
        var tokenExpiryTime = nowTime.AddMinutes(_appConfiguration.Value.TokenExpiryInMinutes);
        var roles= loginInfo.UserRoles.Select(r => r.RoleId).ToList();
        var assignedPermissionCode = await _roleQuery.GetAssignedPermissionCodesAsync(roles, ct);
        var claims = new List<Claim>
        {
            new("name", loginInfo.Name),
            new("email", loginInfo.Email),
            new("sub", loginInfo.UserId.ToString()),
            new("user_id", loginInfo.UserId.ToString())
        };

        // 添加权限到 claims，FastEndpoints 会自动处理权限验证
        if (assignedPermissionCode != null)
        {
            foreach (var permissionCode in assignedPermissionCode)
            {
                claims.Add(new Claim("permissions", permissionCode));
            }
        }

        // 使用 FastEndpoints 的 JWT 生成方式
        var token = await _jwtProvider.GenerateJwtToken(new JwtData("issuer-x", "audience-y", claims, nowTime, tokenExpiryTime), ct);

        var response = new LoginResponse(
            token,
            refreshToken,
            loginInfo.UserId,
            loginInfo.Name,
            loginInfo.Email,
            JsonSerializer.Serialize(assignedPermissionCode),
            tokenExpiryTime
        );

        // 更新用户登录时间
        var updateCmd = new UpdateUserLoginTimeCommand(loginInfo.UserId, DateTime.UtcNow, refreshToken);
        await _mediator.Send(updateCmd, ct);


        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}


public class LoginSummary : Summary<LoginEndpoint>
{
    public LoginSummary()
    {
        Response<LoginResponse>(200, "登录");
    }
}