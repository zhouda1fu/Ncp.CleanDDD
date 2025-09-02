using FluentValidation;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;
using NetCorePal.Extensions.Primitives;

namespace Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;

public record UpdateOrganizationUnitCommand(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId ParentId, int SortOrder) : ICommand;

public class UpdateOrganizationUnitCommandValidator : AbstractValidator<UpdateOrganizationUnitCommand>
{
    public UpdateOrganizationUnitCommandValidator(OrganizationUnitQuery organizationUnitQuery)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("组织架构ID不能为空");
        RuleFor(x => x.Name).NotEmpty().WithMessage("组织架构名称不能为空");
        RuleFor(x => x.Description).MaximumLength(200).WithMessage("组织架构描述长度不能超过200个字符");
        RuleFor(x => x.ParentId).MustAsync(async (parentId, ct) => 
        {
            if (parentId == new OrganizationUnitId(0)) return true; // 根节点
            return await organizationUnitQuery.DoesOrganizationUnitExist(parentId, ct);
        }).WithMessage("父级组织架构不存在");
        RuleFor(x => x).Must(x => x.ParentId != x.Id).WithMessage("不能将自己设置为父级组织架构");
    }
}

public class UpdateOrganizationUnitCommandHandler(IOrganizationUnitRepository organizationUnitRepository) : ICommandHandler<UpdateOrganizationUnitCommand>
{
    public async Task Handle(UpdateOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var organizationUnit = await organizationUnitRepository.GetAsync(request.Id, cancellationToken) ??
                               throw new KnownException($"未找到组织架构，Id = {request.Id}");

        organizationUnit.UpdateInfo(
            request.Name,
            request.Description,
            request.ParentId,
            request.SortOrder);
    }
} 