using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;

[Tags("Roles")]
public class GetAllRoleEndpoint : Endpoint<RoleQueryInput, ResponseData<PagedData<RoleQueryDto>?>>
{
    private readonly RoleQuery _roleQuery;

    public GetAllRoleEndpoint(RoleQuery roleQuery)
    {
        _roleQuery = roleQuery;
    }

    public override void Configure()
    {
        Get("/api/roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.RoleView);
    }

    public override async Task HandleAsync(RoleQueryInput req, CancellationToken ct)
    {
        var roleInfo = await _roleQuery.GetAllRolesAsync(req, ct) ?? throw new KnownException("Invalid Credentials.");
        await Send.OkAsync(roleInfo.AsResponseData(), cancellation: ct);
    }
} 