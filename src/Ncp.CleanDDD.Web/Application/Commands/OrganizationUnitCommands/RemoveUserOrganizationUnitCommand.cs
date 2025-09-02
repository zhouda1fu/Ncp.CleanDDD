using FluentValidation;
using MediatR;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;
using NetCorePal.Extensions.Primitives;

namespace Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;

public record RemoveUserOrganizationUnitCommand(UserId UserId) : ICommand;

public class RemoveUserOrganizationUnitCommandValidator : AbstractValidator<RemoveUserOrganizationUnitCommand>
{
    public RemoveUserOrganizationUnitCommandValidator(UserQuery userQuery)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.UserId).MustAsync(async (userId, ct) => await userQuery.DoesUserExist(userId, ct))
            .WithMessage("用户不存在");
    }
}

public class RemoveUserOrganizationUnitCommandHandler(IUserRepository userRepository) : ICommandHandler<RemoveUserOrganizationUnitCommand>
{
    public async Task Handle(RemoveUserOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}");

       
    }
} 