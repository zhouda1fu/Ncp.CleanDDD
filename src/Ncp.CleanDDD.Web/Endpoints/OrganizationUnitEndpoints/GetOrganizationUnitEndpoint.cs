using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.OrganizationUnitEndpoints;

public record GetOrganizationUnitResponse(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId ParentId, int SortOrder, bool IsActive, DateTime CreatedAt);

[Tags("OrganizationUnits")]
public class GetOrganizationUnitEndpoint : EndpointWithoutRequest<ResponseData<GetOrganizationUnitResponse?>>
{
    private readonly OrganizationUnitQuery _organizationUnitQuery;

    public GetOrganizationUnitEndpoint(OrganizationUnitQuery organizationUnitQuery)
    {
        _organizationUnitQuery = organizationUnitQuery;
    }

    public override void Configure()
    {
        Get("/api/organization-units/{organizationUnitId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.OrganizationUnitView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var organizationUnitId = Route<long>("organizationUnitId");
        var organizationUnit = await _organizationUnitQuery.GetOrganizationUnitByIdAsync(new OrganizationUnitId(organizationUnitId), ct) ?? 
                               throw new KnownException("组织架构不存在");
        
        var response = new GetOrganizationUnitResponse(
            organizationUnit.Id,
            organizationUnit.Name,
            organizationUnit.Description,
            organizationUnit.ParentId,
            organizationUnit.SortOrder,
            organizationUnit.IsActive,
            organizationUnit.CreatedAt
        );

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
} 