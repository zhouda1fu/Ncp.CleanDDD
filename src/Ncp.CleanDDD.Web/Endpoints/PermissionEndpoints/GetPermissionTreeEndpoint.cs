using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;
using System.Security;

namespace Ncp.CleanDDD.Web.Endpoints.RoleEndpoints;

[Tags("Permissions")]
public class GetPermissionTreeEndpoint : EndpointWithoutRequest<ResponseData<IEnumerable<AppPermissionGroup>>>
{
    public override void Configure()
    {
        Get("/api/permissions/tree");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.UserRoleAssign);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var PermissionGroups = PermissionDefinitionContext.PermissionGroups;
        await Send.OkAsync(new ResponseData<IEnumerable<AppPermissionGroup>>(PermissionGroups), cancellation: ct);  
    }
} 