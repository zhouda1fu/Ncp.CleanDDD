using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

public record CreateOrganizationUnitRequest(string Name, string Description, OrganizationUnitId? ParentId, int SortOrder);

public record CreateOrganizationUnitResponse(OrganizationUnitId Id, string Name, string Description);

[Tags("OrganizationUnits")]
public class CreateOrganizationUnitEndpoint : Endpoint<CreateOrganizationUnitRequest, ResponseData<CreateOrganizationUnitResponse>>
{
    private readonly IMediator _mediator;

    public CreateOrganizationUnitEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/organization-units");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.OrganizationUnitCreate);
    }

    public override async Task HandleAsync(CreateOrganizationUnitRequest req, CancellationToken ct)
    {
        var command = new CreateOrganizationUnitCommand(
            req.Name,
            req.Description,
            req.ParentId ?? new OrganizationUnitId(0),
            req.SortOrder
        );

        var organizationUnitId = await _mediator.Send(command, ct);

        var response = new CreateOrganizationUnitResponse(
            organizationUnitId,
            req.Name,
            req.Description
        );

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}