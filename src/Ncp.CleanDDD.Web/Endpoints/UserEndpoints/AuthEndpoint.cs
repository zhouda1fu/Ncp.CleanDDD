using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

[Tags("Users")]
[Authorize(AuthenticationSchemes = "Bearer")]
[HttpGet("/api/user/auth")]
public class AuthEndpoint : EndpointWithoutRequest<ResponseData<bool>>
{
    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}