using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Infrastructure.Repositories;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Primitives;
using Ncp.CleanDDD.Infrastructure;

namespace Ncp.CleanDDD.Web.Application.Queries;

public record OrganizationUnitQueryDto(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId ParentId, int SortOrder, bool IsActive, DateTimeOffset CreatedAt, DateTimeOffset? DeletedAt);

public class OrganizationUnitQueryInput
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public OrganizationUnitId? ParentId { get; set; }
}
/// <summary>
/// 组织架构树形DTO - 应用层数据传输对象
/// </summary>
public record OrganizationUnitTreeDto(
    OrganizationUnitId Id,
    string Name,
    string Description,
    OrganizationUnitId ParentId,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IEnumerable<OrganizationUnitTreeDto> Children);
public class OrganizationUnitQuery(ApplicationDbContext applicationDbContext) : IQuery
{
    private DbSet<OrganizationUnit> OrganizationUnitSet { get; } = applicationDbContext.OrganizationUnits;

    public async Task<bool> DoesOrganizationUnitExist(string name, CancellationToken cancellationToken)
    {
        return await OrganizationUnitSet.AsNoTracking()
            .AnyAsync(ou => ou.Name == name, cancellationToken: cancellationToken);
    }

    public async Task<bool> DoesOrganizationUnitExist(OrganizationUnitId id, CancellationToken cancellationToken)
    {
        return await OrganizationUnitSet.AsNoTracking()
            .AnyAsync(ou => ou.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<OrganizationUnitQueryDto?> GetOrganizationUnitByIdAsync(OrganizationUnitId id, CancellationToken cancellationToken = default)
    {
        return await OrganizationUnitSet.AsNoTracking()
            .Where(ou => ou.Id == id)
            .Select(ou => new OrganizationUnitQueryDto(ou.Id, ou.Name, ou.Description, ou.ParentId, ou.SortOrder, ou.IsActive, ou.CreatedAt, ou.DeletedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrganizationUnitQueryDto>> GetAllOrganizationUnitsAsync(OrganizationUnitQueryInput query, CancellationToken cancellationToken)
    {
        return await OrganizationUnitSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Name), r => r.Name.Contains(query.Name!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.Description), r => r.Description.Contains(query.Description!))
            .WhereIf(query.IsActive.HasValue, r => r.IsActive == query.IsActive)
            .WhereIf(query.ParentId!=null, r => r.ParentId == query.ParentId)
            .OrderBy(ou => ou.SortOrder)
            .Select(ou => new OrganizationUnitQueryDto(ou.Id, ou.Name, ou.Description, ou.ParentId, ou.SortOrder, ou.IsActive, ou.CreatedAt, ou.DeletedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrganizationUnitTreeDto>> GetOrganizationUnitTreeAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var allOrganizations = await OrganizationUnitSet.AsNoTracking()
            .ToListAsync(cancellationToken);

        // 构建树形结构
        var treeStructure = BuildTreeStructure(allOrganizations, includeInactive);
        
        // 转换为应用层DTO
        return treeStructure.Select(ou => ConvertToTreeDto(ou));
    }

    /// <summary>
    /// 构建组织架构树形结构
    /// </summary>
    /// <param name="allOrganizations">所有组织架构集合</param>
    /// <param name="includeInactive">是否包含非激活的组织架构</param>
    /// <returns>树形结构的组织架构集合</returns>
    private static IEnumerable<OrganizationUnit> BuildTreeStructure(
        IEnumerable<OrganizationUnit> allOrganizations,
        bool includeInactive = false)
    {
        var organizationDict = allOrganizations.ToDictionary(ou => ou.Id);
        var result = new List<OrganizationUnit>();

        foreach (var org in allOrganizations)
        {
            if (!includeInactive && !org.IsActive)
                continue;

            // 只处理根节点（ParentId为0）
            if (org.ParentId == new OrganizationUnitId(0))
            {
                result.Add(BuildTreeDto(org, organizationDict, includeInactive));
            }
        }

        return result.OrderBy(ou => ou.SortOrder);
    }

    /// <summary>
    /// 构建单个组织架构的树形结构
    /// </summary>
    /// <param name="organizationUnit">组织架构实体</param>
    /// <param name="allOrganizations">所有组织架构字典</param>
    /// <param name="includeInactive">是否包含非激活的组织架构</param>
    /// <returns>树形结构的组织架构</returns>
    private static OrganizationUnit BuildTreeDto(
        OrganizationUnit organizationUnit,
        Dictionary<OrganizationUnitId, OrganizationUnit> allOrganizations,
        bool includeInactive)
    {
        var children = new List<OrganizationUnit>();

        // 查找所有以当前组织架构为父级的子组织架构
        var childOrganizations = allOrganizations.Values
            .Where(ou => ou.ParentId == organizationUnit.Id)
            .OrderBy(ou => ou.SortOrder);

        foreach (var child in childOrganizations)
        {
            if (!includeInactive && !child.IsActive)
                continue;

            children.Add(BuildTreeDto(child, allOrganizations, includeInactive));
        }

        // 设置子组织架构
        organizationUnit.Children.Clear();
        foreach (var child in children)
        {
            organizationUnit.Children.Add(child);
        }

        return organizationUnit;
    }

    /// <summary>
    /// 将单个组织架构领域模型转换为树形DTO
    /// </summary>
    /// <param name="organizationUnit">组织架构领域模型</param>
    /// <returns>树形DTO</returns>
    private static OrganizationUnitTreeDto ConvertToTreeDto(OrganizationUnit organizationUnit)
    {
        var children = organizationUnit.Children
            .OrderBy(ou => ou.SortOrder)
            .Select(ou => ConvertToTreeDto(ou))
            .ToList();

        return new OrganizationUnitTreeDto(
            organizationUnit.Id,
            organizationUnit.Name,
            organizationUnit.Description,
            organizationUnit.ParentId,
            organizationUnit.SortOrder,
            organizationUnit.IsActive,
            organizationUnit.CreatedAt,
            children
        );
    }
}