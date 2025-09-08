using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;

namespace Ncp.CleanDDD.Web.Application.Commands.UserCommands;

public record UpdateUserLoginTimeCommand(UserId UserId, DateTimeOffset LoginTime,string RefreshToken) : ICommand;

public class UpdateUserLoginTimeCommandValidator : AbstractValidator<UpdateUserLoginTimeCommand>
{
    public UpdateUserLoginTimeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.LoginTime).NotEmpty().WithMessage("登录时间不能为空");
    }
}

public class UpdateUserLoginTimeCommandHandler(IUserRepository userRepository, UserQuery userQuery) : ICommandHandler<UpdateUserLoginTimeCommand>
{
    public async Task Handle(UpdateUserLoginTimeCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new KnownException($"用户不存在，UserId={request.UserId}");
        }

        user.UpdateLastLoginTime(request.LoginTime);

        user.SetUserRefreshToken(request.RefreshToken);

        // 清除用户相关缓存
        userQuery.ClearUserPermissionCache(request.UserId);
    }
} 