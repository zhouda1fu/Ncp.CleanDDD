using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;

public record GetRoleRequest(RoleId RoleId);

public record GetRoleResponse(RoleId Id, string Name, string Description, bool IsActive, DateTime CreatedAt);

/// <summary>
/// 获取角色信息
/// </summary>
[Tags("Roles")]
public class GetRoleEndpoint : Endpoint<GetRoleRequest, ResponseData<GetRoleResponse?>>
{
    private readonly RoleQuery _roleQuery;

    public GetRoleEndpoint(RoleQuery roleQuery)
    {
        _roleQuery = roleQuery;
    }

    public override void Configure()
    {
        Get("/api/roles/{roleId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.RoleView);
    }

    public override async Task HandleAsync(GetRoleRequest req, CancellationToken ct)
    {
        var roleInfo = await _roleQuery.GetRoleByIdAsync(req.RoleId, ct) ?? throw new KnownException("Invalid Credentials.");
        var response = new GetRoleResponse(
            roleInfo.RoleId,
            roleInfo.Name,
            roleInfo.Description,
            roleInfo.IsActive,
            roleInfo.CreatedAt
        );

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
} 