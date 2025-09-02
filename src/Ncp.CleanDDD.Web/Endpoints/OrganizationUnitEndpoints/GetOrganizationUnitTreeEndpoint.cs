using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

public record GetOrganizationUnitTreeRequest(bool IncludeInactive = false);

[Tags("OrganizationUnits")]
public class GetOrganizationUnitTreeEndpoint : Endpoint<GetOrganizationUnitTreeRequest, ResponseData<IEnumerable<OrganizationUnitTreeDto>?>>
{
    private readonly OrganizationUnitQuery _organizationUnitQuery;

    public GetOrganizationUnitTreeEndpoint(OrganizationUnitQuery organizationUnitQuery)
    {
        _organizationUnitQuery = organizationUnitQuery;
    }

    public override void Configure()
    {
        Get("/api/organization-units/tree");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.OrganizationUnitView);
    }

    public override async Task HandleAsync(GetOrganizationUnitTreeRequest req, CancellationToken ct)
    {
        var organizationUnitTree = await _organizationUnitQuery.GetOrganizationUnitTreeAsync(req.IncludeInactive, ct);
        await Send.OkAsync(organizationUnitTree.AsResponseData(), cancellation: ct);
    }
} 