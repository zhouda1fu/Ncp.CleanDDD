using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;


public record UpdateUserRolesRequest(UserId UserId, IEnumerable<RoleId> RoleIds);

public record UpdateUserRolesResponse(UserId UserId);

[Tags("Users")]
public class UpdateUserRolesEndpoint : Endpoint<UpdateUserRolesRequest,ResponseData<UpdateUserRolesResponse>>
{

    private readonly IMediator _mediator;
    private readonly RoleQuery _roleQuery;

    public UpdateUserRolesEndpoint(IMediator mediator, RoleQuery roleQuery)
    {
        _mediator = mediator;
        _roleQuery = roleQuery;
    }

    public override void Configure()
    {
        Put("/api/users/update-roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.UserRoleAssign);
    }

    public override async Task HandleAsync(UpdateUserRolesRequest request, CancellationToken ct)
    {
        var rolesToBeAssigned = await _roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);

        var cmd = new UpdateUserRolesCommand(request.UserId, rolesToBeAssigned);
        await _mediator.Send(cmd, ct);
        var response = new UpdateUserRolesResponse(
            request.UserId
        );

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}