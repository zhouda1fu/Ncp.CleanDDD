using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

public record UpdateOrganizationUnitRequest( string Name, string Description, OrganizationUnitId? ParentId, int SortOrder);

[Tags("OrganizationUnits")]
public class UpdateOrganizationUnitEndpoint : Endpoint<UpdateOrganizationUnitRequest, ResponseData<bool>>
{
    private readonly IMediator _mediator;

    public UpdateOrganizationUnitEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/organization-units/{organizationUnitId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.OrganizationUnitEdit);
    }

    public override async Task HandleAsync(UpdateOrganizationUnitRequest request, CancellationToken ct)
    {
        var organizationUnitId = Route<OrganizationUnitId>("organizationUnitId") ?? throw new KnownException("组织架构ID不能为空");
        var command = new UpdateOrganizationUnitCommand(
            organizationUnitId,
            request.Name,
            request.Description,
            request.ParentId ?? new OrganizationUnitId(0),
            request.SortOrder
        );

        await _mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}