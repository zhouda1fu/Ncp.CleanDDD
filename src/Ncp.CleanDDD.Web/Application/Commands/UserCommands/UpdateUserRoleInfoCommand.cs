using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;

namespace Ncp.CleanDDD.Web.Application.Commands.UserCommands
{


    public record UpdateUserRoleInfoCommand(UserId UserId, RoleId RoleId, string RoleName) : ICommand;

    public class UpdateUserRoleInfoCommandHandler(IUserRepository userRepository)
        : ICommandHandler<UpdateUserRoleInfoCommand>
    {
        public async Task Handle(UpdateUserRoleInfoCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                       throw new KnownException($"未找到用户，AdminUserId = {request.UserId}");

            user.UpdateRoleInfo(request.RoleId, request.RoleName);
        }
    }
}
