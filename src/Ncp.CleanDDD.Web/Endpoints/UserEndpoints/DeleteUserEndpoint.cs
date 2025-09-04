using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;




[Tags("Users")]
public class DeleteUserEndpoint : EndpointWithoutRequest<ResponseData<bool>>
{
    private readonly IMediator _mediator;

    public DeleteUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/users/{userId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserDelete);

    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Route<UserId>("userId") ?? throw new KnownException("id不能为空");
        var command = new DeleteUserCommand(userId);
        await _mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 删除用户端点的API文档配置
/// </summary>
public class DeleteUserSummary : Summary<DeleteUserEndpoint>
{
    public DeleteUserSummary()
    {
        Summary = "删除用户";
        Description = "根据用户ID删除指定的用户账户";
        ResponseExamples[200] = new ResponseData<bool>(true, true, "用户删除成功");
        Responses[200] = "用户删除成功";
        Responses[401] = "权限不足，无法删除用户";
        Responses[404] = "用户不存在";
    }
}