using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

public record AssignUserOrganizationUnitRequest(UserId UserId, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName);

public record AssignUserOrganizationUnitResponse(UserId UserId);

[Tags("OrganizationUnits")]
public class AssignUserOrganizationUnitEndpoint : Endpoint<AssignUserOrganizationUnitRequest, ResponseData<AssignUserOrganizationUnitResponse>>
{
    private readonly IMediator _mediator;

    public AssignUserOrganizationUnitEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/organization-units/assign-user");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.OrganizationUnitEdit);
    }

    public override async Task HandleAsync(AssignUserOrganizationUnitRequest request, CancellationToken ct)
    {
        var command = new AssignUserOrganizationUnitCommand(
            request.UserId,
            request.OrganizationUnitId,
            request.OrganizationUnitName
        );

        await _mediator.Send(command, ct);

        var response = new AssignUserOrganizationUnitResponse(request.UserId);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
} 