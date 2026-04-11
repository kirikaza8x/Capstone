using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence.Contexts;

namespace Users.Infrastructure.Data.Repositories
{
    public class IOrganizerRepository : RepositoryBase<OrganizerProfile, Guid>, IOrganizerProfileRepository
    {
        private readonly UserModuleDbContext _context;
        private readonly DbSet<OrganizerProfile> _dbSet;

        public IOrganizerRepository(UserModuleDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<OrganizerProfile>();
        }


    }
}
