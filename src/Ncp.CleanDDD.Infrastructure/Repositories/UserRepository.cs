using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;

namespace Ncp.CleanDDD.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User, UserId>;

public class UserRepository(ApplicationDbContext context) : RepositoryBase<User, UserId, ApplicationDbContext>(context), IUserRepository
{
   
} 