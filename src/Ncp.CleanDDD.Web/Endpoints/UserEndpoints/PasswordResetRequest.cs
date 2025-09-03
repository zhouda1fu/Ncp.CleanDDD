using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Utils;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;


public record PasswordResetRequest(UserId UserId);

public record PasswordResetResponse(UserId UserId);

[Tags("Users")]
public class PasswordResetEndpoint : Endpoint<PasswordResetRequest, ResponseData<PasswordResetResponse>>
{

    private readonly IMediator _mediator;
    private readonly RoleQuery _roleQuery;

    public PasswordResetEndpoint(IMediator mediator, RoleQuery roleQuery)
    {
        _mediator = mediator;
        _roleQuery = roleQuery;
    }

    public override void Configure()
    {
        Put("/api/user/password-reset");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        // FastEndpoints 会自动从 JWT claims 中验证权限
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.UserEdit);
    }

    public override async Task HandleAsync(PasswordResetRequest request, CancellationToken ct)
    {
        var passwordHash = PasswordHasher.HashPassword("123456");
        var cmd = new PasswordResetCommand(request.UserId, passwordHash);
        var userId = await _mediator.Send(cmd, ct);
        var response = new PasswordResetResponse(
            userId
        );
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}