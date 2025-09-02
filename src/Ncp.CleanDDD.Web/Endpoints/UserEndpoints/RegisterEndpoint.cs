using FastEndpoints;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Utils;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

public record RegisterRequest(string Name, string Email, string Password, string Phone, string RealName, int Status, string Gender, int Age, DateTime BirthDate, OrganizationUnitId? OrganizationUnitId, string? OrganizationUnitName, IEnumerable<RoleId> RoleIds);

public record RegisterResponse(UserId UserId, string Name, string Email);

[Tags("Users")]
public class RegisterEndpoint : Endpoint<RegisterRequest, ResponseData<RegisterResponse>>
{
    private readonly IMediator _mediator;
    private readonly RoleQuery _roleQuery;

    public RegisterEndpoint(IMediator mediator, RoleQuery roleQuery)
    {
        _mediator = mediator;
        _roleQuery = roleQuery;
    }

    public override void Configure()
    {
        Post("/api/user/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest request, CancellationToken ct)
    {

        var rolesToBeAssigned = await _roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);

        // 哈希密码
        var passwordHash = PasswordHasher.HashPassword(request.Password);
        var cmd = new CreateUserCommand(request.Name, request.Email, passwordHash, request.Phone, request.RealName, request.Status, request.Gender,  request.BirthDate, request.OrganizationUnitId, request.OrganizationUnitName, rolesToBeAssigned);
        var userId = await _mediator.Send(cmd, ct);
        var response = new RegisterResponse(
            userId,
            request.Name,
            request.Email
        );

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}