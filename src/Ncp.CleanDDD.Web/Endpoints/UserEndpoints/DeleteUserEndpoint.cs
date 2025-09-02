using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.AppPermissions;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

/// <summary>
/// 删除请求模型
/// </summary>
/// <param name="UserId">用户id</param>
public record DeleteUserRequest(UserId UserId);


[Tags("Users")]
public class DeleteUserEndpoint : Endpoint<DeleteUserRequest, ResponseData<bool>>
{
    private readonly IMediator _mediator;

    public DeleteUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess,PermissionCodes.UserDelete);
       
    }

    public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        var command = new DeleteUserCommand(req.UserId);
        await _mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 删除用户端点的API文档配置
/// </summary>
public class DeleteUserSummary : Summary<DeleteUserEndpoint, DeleteUserRequest>
{
    public DeleteUserSummary()
    {
        Summary = "删除用户";
        Description = "根据用户ID删除指定的用户账户";
        ExampleRequest = new DeleteUserRequest(new UserId(0));
        ResponseExamples[200] = new ResponseData<bool>(true, true, "用户删除成功");
        Responses[200] = "用户删除成功";
        Responses[401] = "权限不足，无法删除用户";
        Responses[404] = "用户不存在";
    }
}