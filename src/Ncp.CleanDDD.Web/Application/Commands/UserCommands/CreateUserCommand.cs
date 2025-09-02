using FluentValidation;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;
using NetCorePal.Extensions.Primitives;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;

namespace Ncp.CleanDDD.Web.Application.Commands.UserCommands;

public record CreateUserCommand(string Name, string Email, string Password, string Phone, string RealName, int Status, string Gender,  DateTime BirthDate, OrganizationUnitId? OrganizationUnitId, string? OrganizationUnitName, IEnumerable<AssignAdminUserRoleQueryDto> RolesToBeAssigned) : ICommand<UserId>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(UserQuery userQuery)
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage("用户名不能为空");
        //RuleFor(u => u.Email).NotEmpty().EmailAddress().WithMessage("邮箱格式不正确");
        RuleFor(u => u.Password).NotEmpty().WithMessage("密码不能为空");
        RuleFor(u => u.Name).MustAsync(async (n, ct) => !await userQuery.DoesUserExist(n, ct))
            .WithMessage(u => $"该用户已存在，Name={u.Name}");
        //RuleFor(u => u.Email).MustAsync(async (e, ct) => !await userQuery.DoesEmailExist(e, ct))
        //    .WithMessage(u => $"该邮箱已存在，Email={u.Email}");
    }
}

public class CreateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {

        List<UserRole> userRoles = [];

        foreach (var (roleId, roleName, permissionCodes) in request.RolesToBeAssigned)
        {
            userRoles.Add(new UserRole(roleId, roleName));
        }

        var user = new User(request.Name, request.Phone,
            request.Password,
            userRoles, request.RealName, request.Status, request.Email, request.Gender, request.BirthDate);

        // 分配组织架构
        if (request.OrganizationUnitId != null && !string.IsNullOrEmpty(request.OrganizationUnitName))
        {
            var organizationUnit = new UserOrganizationUnit(user.Id, request.OrganizationUnitId, request.OrganizationUnitName);
            user.AssignOrganizationUnit(organizationUnit);
        }

        await userRepository.AddAsync(user, cancellationToken);
        return user.Id;
    }

}