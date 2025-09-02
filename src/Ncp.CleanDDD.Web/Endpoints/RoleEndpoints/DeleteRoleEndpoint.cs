using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;


[Tags("Roles")]
public class DeleteRoleEndpoint : EndpointWithoutRequest<ResponseData<bool>>
{
    private readonly IMediator _mediator;

    public DeleteRoleEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/roles/{roleId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.UserDelete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var roleId = Route<RoleId>("roleId") ?? throw new KnownException("id²»ÄÜÎª¿Õ");
        var command = new DeleteRoleCommand(roleId);
        await _mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}