using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;

namespace Ncp.CleanDDD.Web.Application.Commands.UserCommands
{


    public record PasswordResetCommand(UserId UserId, string Password) : ICommand<UserId>;

    public class PasswordResetCommandValidator : AbstractValidator<PasswordResetCommand>
    {
        public PasswordResetCommandValidator(UserQuery userQuery)
        {
            RuleFor(u => u.UserId).NotEmpty().WithMessage("userid不能为空");
            RuleFor(u => u.Password).NotEmpty().WithMessage("密码不能为空");
        }
    }

    public class PasswordResetCommandHandler(IUserRepository userRepository) : ICommandHandler<PasswordResetCommand, UserId>
    {
        public async Task<UserId> Handle(PasswordResetCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetAsync(request.UserId, cancellationToken) ?? throw new KnownException($"用户不存在，UserId={request.UserId}");
            user.PasswordReset(request.Password);
            return user.Id;
        }

    }
}
