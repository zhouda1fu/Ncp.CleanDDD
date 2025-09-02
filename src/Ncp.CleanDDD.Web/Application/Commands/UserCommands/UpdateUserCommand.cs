using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;

namespace Ncp.CleanDDD.Web.Application.Commands.UserCommands
{


    public record UpdateUserCommand(UserId UserId, string Name, string Email, string Phone, string RealName, int Status, string Gender, int Age, DateTime BirthDate, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName,string PasswordHash) : ICommand<UserId>;

    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator(UserQuery userQuery)
        {
            RuleFor(u => u.Name).NotEmpty().WithMessage("用户名不能为空");
            //RuleFor(u => u.Email).NotEmpty().EmailAddress().WithMessage("邮箱格式不正确");
            // RuleFor(u => u.Password).NotEmpty().WithMessage("密码不能为空");
            //RuleFor(u => u.Email).MustAsync(async (e, ct) => !await userQuery.DoesEmailExist(e, ct))
            //    .WithMessage(u => $"该邮箱已存在，Email={u.Email}");
        }
    }

    public class UpdateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserCommand, UserId>
    {
        public async Task<UserId> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new KnownException($"用户不存在，UserId={request.UserId}");
            }

            user.UpdateUserInfo(request.Name, request.Phone,
                request.RealName, request.Status, request.Email, request.Gender, request.BirthDate);

            user.UpdatePassword(request.PasswordHash);

            // 更新组织架构

            var organizationUnit = new UserOrganizationUnit(user.Id, request.OrganizationUnitId, request.OrganizationUnitName);
            user.AssignOrganizationUnit(organizationUnit);
            return user.Id;
        }

    }
}
