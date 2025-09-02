using FluentValidation;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using Ncp.CleanDDD.Web.Application.Queries;
using NetCorePal.Extensions.Primitives;

namespace Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;

public record CreateOrganizationUnitCommand(string Name, string Description, OrganizationUnitId ParentId, int SortOrder) : ICommand<OrganizationUnitId>;

public class CreateOrganizationUnitCommandValidator : AbstractValidator<CreateOrganizationUnitCommand>
{
    public CreateOrganizationUnitCommandValidator(OrganizationUnitQuery organizationUnitQuery)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("组织架构名称不能为空");
        RuleFor(x => x.Description).MaximumLength(200).WithMessage("组织架构描述长度不能超过200个字符");
        RuleFor(x => x.Name).MustAsync(async (n, ct) => !await organizationUnitQuery.DoesOrganizationUnitExist(n, ct))
            .WithMessage(x => $"该组织架构已存在，Name={x.Name}");
        RuleFor(x => x.ParentId).MustAsync(async (parentId, ct) => 
        {
            if (parentId == new OrganizationUnitId(0)) return true; // 根节点
            return await organizationUnitQuery.DoesOrganizationUnitExist(parentId, ct);
        }).WithMessage("父级组织架构不存在");
    }
}

public class CreateOrganizationUnitCommandHandler(IOrganizationUnitRepository organizationUnitRepository) : ICommandHandler<CreateOrganizationUnitCommand, OrganizationUnitId>
{
    public async Task<OrganizationUnitId> Handle(CreateOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var organizationUnit = new OrganizationUnit(
            request.Name,
            request.Description,
            request.ParentId,
            request.SortOrder);

        await organizationUnitRepository.AddAsync(organizationUnit, cancellationToken);
        return organizationUnit.Id;
    }
} 