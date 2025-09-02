using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

public record RemoveUserOrganizationUnitRequest(UserId UserId);

public record RemoveUserOrganizationUnitResponse(UserId UserId);

[Tags("OrganizationUnits")]
public class RemoveUserOrganizationUnitEndpoint : Endpoint<RemoveUserOrganizationUnitRequest, ResponseData<RemoveUserOrganizationUnitResponse>>
{
    private readonly IMediator _mediator;

    public RemoveUserOrganizationUnitEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/organization-units/remove-user");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.OrganizationUnitManagement);
    }

    public override async Task HandleAsync(RemoveUserOrganizationUnitRequest request, CancellationToken ct)
    {
        var cmd = new RemoveUserOrganizationUnitCommand(request.UserId);
        await _mediator.Send(cmd, ct);
        
        var response = new RemoveUserOrganizationUnitResponse(request.UserId);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
} 