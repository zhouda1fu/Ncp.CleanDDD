using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

public record GetUserProfileRequest(UserId UserId);

public record UserProfileResponse(UserId UserId, string Name, string Phone, IEnumerable<string> Roles, string RealName, int Status, string Email, DateTime CreatedAt, string Gender, int Age,DateTime BirthDate, OrganizationUnitId? OrganizationUnitId,string OrganizationUnitName);

[Tags("Users")]
public class GetUserProfileEndpoint : Endpoint<GetUserProfileRequest, ResponseData<UserProfileResponse?>>
{
    private readonly UserQuery _userQuery;

    public GetUserProfileEndpoint(UserQuery userQuery)
    {
        _userQuery = userQuery;
    }

    public override void Configure()
    {
        Get("/api/user/profile/{userId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.UserView);
    }

    public override async Task HandleAsync(GetUserProfileRequest req, CancellationToken ct)
    {
        var userInfo = await _userQuery.GetUserByIdAsync(req.UserId, ct);

        if (userInfo == null)
        {
            throw new KnownException("无效的用户");
        }

        var response = new UserProfileResponse(
            userInfo.UserId,
            userInfo.Name,
            userInfo.Phone,
            userInfo.Roles,
            userInfo.RealName,
            userInfo.Status,
            userInfo.Email,
            userInfo.CreatedAt,
            userInfo.Gender,
            userInfo.Age,
            userInfo.BirthDate,
            userInfo.OrganizationUnitId,
            userInfo.OrganizationUnitName
        );

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 获取用户资料端点的API文档配置
/// </summary>
public class GetUserProfileSummary : Summary<GetUserProfileEndpoint, GetUserProfileRequest>
{
    public GetUserProfileSummary()
    {
        Summary = "获取用户资料";
        Description = "根据用户ID获取用户的详细资料信息，包括基本信息、角色、状态等";
        ExampleRequest = new GetUserProfileRequest(new UserId(0));
        ResponseExamples[200] = new ResponseData<UserProfileResponse>
        (
           new UserProfileResponse(
                new UserId(0),
                "testuser",
                "13800138000",
                new[] { "Admin", "User" },
                "测试用户",
                1,
                "test@example.com",
                DateTime.Now,
                "男",
                25, 
                DateTime.Now.AddYears(-25),
                null,
                ""
            ),
             true,
           "获取用户资料成功"
        );
        Responses[200] = "成功获取用户资料";
        Responses[401] = "权限不足，无法查看用户资料";
        Responses[404] = "用户不存在";
    }
}