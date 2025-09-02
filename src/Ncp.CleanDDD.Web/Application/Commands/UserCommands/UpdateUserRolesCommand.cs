using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;

namespace Ncp.CleanDDD.Web.Application.Commands.UserCommands
{


    public record UpdateUserRolesCommand(UserId UserId, List<AssignAdminUserRoleQueryDto> RolesToBeAssigned)
    : ICommand;

    public class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
    {
        public UpdateUserRolesCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class UpdateUserRolesCommandHandler(IUserRepository userRepository)
        : ICommandHandler<UpdateUserRolesCommand>
    {
        public async Task Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            var adminUser = await userRepository.GetAsync(request.UserId, cancellationToken)
                            ?? throw new KnownException($"未找到用户，UserId = {request.UserId}");

            List<UserRole> roles = [];

            foreach (var assignAdminUserRoleDto in request.RolesToBeAssigned)
            {
                roles.Add(new UserRole(assignAdminUserRoleDto.RoleId, assignAdminUserRoleDto.RoleName));
            }

            adminUser.UpdateRoles(roles);
        }
    }
}
