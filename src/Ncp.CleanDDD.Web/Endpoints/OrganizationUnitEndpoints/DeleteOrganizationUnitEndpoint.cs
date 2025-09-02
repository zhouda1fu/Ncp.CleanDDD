using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

[Tags("OrganizationUnits")]
public class DeleteOrganizationUnitEndpoint : EndpointWithoutRequest<ResponseData<bool>>
{
    private readonly IMediator _mediator;

    public DeleteOrganizationUnitEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/organization-units/{organizationUnitId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.OrganizationUnitDelete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var organizationUnitId = Route<OrganizationUnitId>("organizationUnitId") ?? throw new KnownException("id²»ÄÜÎª¿Õ"); ;
        var command = new DeleteOrganizationUnitCommand(organizationUnitId);
        await _mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
} 