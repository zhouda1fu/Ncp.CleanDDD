using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;



/// <summary>
/// 获取所有用户信息
/// </summary>
public class GetAllUsersEndpoint : Endpoint<UserQueryInput, ResponseData<PagedData<UserInfoQueryDto>>>
{
    private readonly UserQuery _userQuery;

    public GetAllUsersEndpoint(UserQuery userQuery)
    {
        _userQuery = userQuery;
    }

    public override void Configure()
    {
        Get("/api/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        //AllowAnonymous();
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.UserView);
        Tags("Users");
    }

    public override async Task HandleAsync(UserQueryInput req, CancellationToken ct)
    {
        var result = await _userQuery.GetAllUsersAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }

    public class GetAllUsersSummary : Summary<GetAllUsersEndpoint>
    {
        public GetAllUsersSummary()
        {
            Response<UserInfoQueryDto>(200, "用户信息");
        }
    }
}