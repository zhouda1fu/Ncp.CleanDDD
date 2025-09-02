using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

[Tags("OrganizationUnits")]
public class GetAllOrganizationUnitsEndpoint : Endpoint<OrganizationUnitQueryInput, ResponseData<IEnumerable<OrganizationUnitQueryDto>?>>
{
    private readonly OrganizationUnitQuery _organizationUnitQuery;

    public GetAllOrganizationUnitsEndpoint(OrganizationUnitQuery organizationUnitQuery)
    {
        _organizationUnitQuery = organizationUnitQuery;
    }

    public override void Configure()
    {
        Get("/api/organization-units");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.OrganizationUnitView);
    }

    public override async Task HandleAsync(OrganizationUnitQueryInput req, CancellationToken ct)
    {
        var organizationUnits = await _organizationUnitQuery.GetAllOrganizationUnitsAsync(req, ct);
        await Send.OkAsync(organizationUnits.AsResponseData(), cancellation: ct);
    }
} 