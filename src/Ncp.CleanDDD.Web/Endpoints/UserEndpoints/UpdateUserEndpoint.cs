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


public record UpdateUserRequest(UserId UserId, string Name, string Email, string Phone, string RealName, int Status, string Gender, int Age, DateTime BirthDate, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName, string Password);

public record UpdateUserResponse(UserId UserId, string Name, string Email);

[Tags("Users")]
public class UpdateUserEndpoint : Endpoint<UpdateUserRequest, ResponseData<UpdateUserResponse>>
{

    private readonly IMediator _mediator;
    private readonly RoleQuery _roleQuery;

    public UpdateUserEndpoint(IMediator mediator, RoleQuery roleQuery)
    {
        _mediator = mediator;
        _roleQuery = roleQuery;
    }

    public override void Configure()
    {
        Put("/api/user/update");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        // FastEndpoints 会自动从 JWT claims 中验证权限
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.UserEdit);
    }

    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {
        var passwordHash = string.Empty;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            passwordHash = PasswordHasher.HashPassword(request.Password);
        }
        var cmd = new UpdateUserCommand(request.UserId, request.Name, request.Email, request.Phone, request.RealName, request.Status, request.Gender, request.Age, request.BirthDate, request.OrganizationUnitId, request.OrganizationUnitName, passwordHash);
        var userId = await _mediator.Send(cmd, ct);
        var response = new UpdateUserResponse(
            userId,
            request.Name,
            request.Email
        );
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}