using FluentValidation;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using NetCorePal.Extensions.Primitives;

namespace Ncp.CleanDDD.Web.Application.Commands.OrganizationUnitCommands;

public record DeleteOrganizationUnitCommand(OrganizationUnitId Id) : ICommand;

public class DeleteOrganizationUnitCommandValidator : AbstractValidator<DeleteOrganizationUnitCommand>
{
    public DeleteOrganizationUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("组织架构ID不能为空");
    }
}

public class DeleteOrganizationUnitCommandHandler(IOrganizationUnitRepository organizationUnitRepository) : ICommandHandler<DeleteOrganizationUnitCommand>
{
    public async Task Handle(DeleteOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var organizationUnit = await organizationUnitRepository.GetAsync(request.Id, cancellationToken) ??
                               throw new KnownException($"未找到组织架构，Id = {request.Id}");

        // 检查是否为根组织架构，防止删除根节点
        //if (organizationUnit.ParentId == new OrganizationUnitId(0))
        //{
        //    throw new KnownException("不能删除根组织架构");
        //}

        organizationUnit.Delete();
    }
} 