using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Utils;

namespace Ncp.CleanDDD.Web.Application.Queries;

/// <summary>
/// 用户信息查询Dto
/// </summary>
/// <param name="UserId">ID</param>
/// <param name="Name">用户名</param>
/// <param name="Phone">手机号</param>
/// <param name="Roles">角色</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">状态</param>
/// <param name="Email">邮箱</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="Gender">性别</param>
/// <param name="Age">年龄</param>
/// <param name="OrganizationUnitId">组织架构id</param>
/// <param name="OrganizationUnitName">组织架构名称</param>
/// <param name="BirthDate">出生日期</param>
/// <param name="IdType">ID类型</param>
/// <param name="IdNo">ID</param>
/// <param name="Cosplay">角色扮演</param>
public record UserInfoQueryDto(UserId UserId, string Name, string Phone, IEnumerable<string> Roles, string RealName, int Status, string Email, DateTimeOffset CreatedAt, string Gender, int Age, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName, DateTimeOffset BirthDate);

public record UserLoginInfoQueryDto(UserId UserId, string Name, string Email, string PasswordHash, IEnumerable<UserRole> UserRoles);

/// <summary>
/// 获取用户信息入参
/// </summary>
public class UserQueryInput : PageRequest
{
    /// <summary>
    /// 用户姓名
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 组织架构id
    /// </summary>
    public OrganizationUnitId? OrganizationUnitId { get; set; }
}

public class UserQuery(ApplicationDbContext applicationDbContext, IMemoryCache memoryCache) : IQuery
{
    private DbSet<User> UserSet { get; } = applicationDbContext.Users;


    public async Task<UserId> GetUserIdByRefreshTokenAsync(string refreshToken,
      CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
                   .SelectMany(u => u.RefreshTokens)
                   .Where(t => t.Token == refreshToken)
                   .Select(t => t.UserId)
                   .SingleOrDefaultAsync(cancellationToken)
               ?? throw new KnownException("无效的令牌");
    }


    public async Task<bool> DoesUserExist(string username, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Name == username, cancellationToken: cancellationToken);
    }




    public async Task<bool> DoesUserExist(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken: cancellationToken);
    }

    public async Task<bool> DoesEmailExist(string email, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken: cancellationToken);
    }

    public async Task<UserInfoQueryDto?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(au => new UserInfoQueryDto(au.Id, au.Name, au.Phone, au.Roles.Select(r => r.RoleName), au.RealName, au.Status, au.Email, au.CreatedAt, au.Gender, au.Age, au.OrganizationUnit.OrganizationUnitId, au.OrganizationUnit.OrganizationUnitName, au.BirthDate))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserInfoQueryDto>> GetUserListByIdsAsync(IEnumerable<UserId> ids, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(au => new UserInfoQueryDto(au.Id, au.Name, au.Phone, au.Roles.Select(r => r.RoleName), au.RealName, au.Status, au.Email, au.CreatedAt, au.Gender, au.Age, au.OrganizationUnit.OrganizationUnitId, au.OrganizationUnit.OrganizationUnitName, au.BirthDate))
            .ToListAsync(cancellationToken);
    }




    public async Task<List<UserId>> GetUserIdsByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken = default)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Roles.Any(r => r.RoleId == roleId))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginAsync(string name, CancellationToken cancellationToken)
    {
        return await UserSet
        .Where(u => u.Name == name)
        .Select(u => new UserLoginInfoQueryDto(u.Id, u.Name, u.Email, u.PasswordHash, u.Roles))
        .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet
        .Where(u => u.Id == userId)
        .Select(u => new UserLoginInfoQueryDto(u.Id, u.Name, u.Email, u.PasswordHash, u.Roles))
        .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 清除用户权限缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    public void ClearUserPermissionCache(UserId userId)
    {
        var cacheKey = $"{CacheKeys.UserPermissions}:{userId}";
        memoryCache.Remove(cacheKey);
    }

    public async Task<PagedData<UserInfoQueryDto>> GetAllUsersAsync(UserQueryInput query, CancellationToken cancellationToken)
    {
        var queryable = UserSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            queryable = queryable.Where(u => u.Name.Contains(query.Keyword!) || u.Email.Contains(query.Keyword!));
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(u => u.Status == query.Status);
        }

        if (query.OrganizationUnitId != null)
        {
            queryable = queryable.Where(u => u.OrganizationUnit != null && u.OrganizationUnit.OrganizationUnitId == query.OrganizationUnitId);
        }

        return await queryable
            .OrderByDescending(u => u.Id)
            .Select(u => new UserInfoQueryDto(
                u.Id,
                u.Name,
                u.Phone,
                u.Roles.Select(r => r.RoleName),
                u.RealName,
                u.Status,
                u.Email,
                u.CreatedAt,
                u.Gender,
                u.Age,
                u.OrganizationUnit.OrganizationUnitId, u.OrganizationUnit.OrganizationUnitName, u.BirthDate))
            .ToPagedDataAsync(query, cancellationToken);
    }
}