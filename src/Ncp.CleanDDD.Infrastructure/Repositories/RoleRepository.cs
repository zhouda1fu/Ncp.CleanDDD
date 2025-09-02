using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;

namespace Ncp.CleanDDD.Infrastructure.Repositories;

public interface IRoleRepository : IRepository<Role, RoleId>;

public class RoleRepository : RepositoryBase<Role, RoleId, ApplicationDbContext>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

  
} 