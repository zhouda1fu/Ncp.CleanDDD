using FluentValidation;
using MediatR;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;
using NetCorePal.Extensions.Primitives;
using System.Security.Cryptography;

namespace Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;

public record AssignUserOrganizationUnitCommand(UserId UserId, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName) : ICommand;

public class AssignUserOrganizationUnitCommandValidator : AbstractValidator<AssignUserOrganizationUnitCommand>
{
    public AssignUserOrganizationUnitCommandValidator(UserQuery userQuery, OrganizationUnitQuery organizationUnitQuery)
    {
        
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.OrganizationUnitId).NotEmpty().WithMessage("组织架构ID不能为空");
        RuleFor(x => x.OrganizationUnitName).NotEmpty().WithMessage("组织架构名称不能为空");
        RuleFor(x => x.UserId).MustAsync(async (userId, ct) => await userQuery.DoesUserExist(userId, ct))
            .WithMessage("用户不存在");
        RuleFor(x => x.OrganizationUnitId).MustAsync(async (orgId, ct) => await organizationUnitQuery.DoesOrganizationUnitExist(orgId, ct))
            .WithMessage("组织架构不存在");

    }
}

public class AssignUserOrganizationUnitCommandHandler(IUserRepository userRepository) : ICommandHandler<AssignUserOrganizationUnitCommand>
{
    public async Task Handle(AssignUserOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}");

        // 创建用户组织架构关联
        var userOrganizationUnit = new UserOrganizationUnit(
                request.UserId,
                request.OrganizationUnitId,
                request.OrganizationUnitName
            );

        // 更新用户的组织架构
        user.AssignOrganizationUnit(userOrganizationUnit);
    }
}