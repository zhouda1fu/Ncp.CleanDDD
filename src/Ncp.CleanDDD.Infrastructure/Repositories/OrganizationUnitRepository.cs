using Microsoft.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;

namespace Ncp.CleanDDD.Infrastructure.Repositories
{
    public interface IOrganizationUnitRepository : IRepository<OrganizationUnit, OrganizationUnitId>;

    public class OrganizationUnitRepository : RepositoryBase<OrganizationUnit, OrganizationUnitId, ApplicationDbContext>, IOrganizationUnitRepository
    {
        public OrganizationUnitRepository(ApplicationDbContext context) : base(context)
        {
        }


    }

} 