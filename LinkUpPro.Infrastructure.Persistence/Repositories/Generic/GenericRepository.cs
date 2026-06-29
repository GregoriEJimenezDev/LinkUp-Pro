
using LinkUpPro.Core.Domain.Interfaces.IGeneric;
using LinkUpPro.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories.Generic
{
    public class GenericRepository<Entity> : IGenericRepository<Entity> where Entity : class
    {
        protected readonly LinkUpProDbContext _context;
        protected readonly DbSet<Entity> _dbSet;
        public GenericRepository(LinkUpProDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Entity>();
        }

        public async Task AddRangeAsync(IEnumerable<Entity> entities)
        {
             _dbSet.AddRange(entities);
        }

        public async Task<Entity?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity;
        }

        public async Task<IEnumerable<Entity>> GetAllAsync() =>
            await _dbSet.AsNoTracking().ToListAsync();

        public async Task AddAsync(Entity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Entity entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Entity entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
