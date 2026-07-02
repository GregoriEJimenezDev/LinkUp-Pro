namespace LinkUpPro.Core.Domain.Interfaces.IGeneric
{
    public interface IGenericRepository<Entity> where Entity : class
    {
        Task<Entity?> GetByIdAsync(int id);
        Task<IEnumerable<Entity>> GetAllAsync();
        Task AddAsync(Entity entity);
        Task AddRangeAsync(IEnumerable<Entity> entities);
        Task UpdateAsync(Entity entity);
        Task UpdateRangeAsync(IEnumerable<Entity> entities);
        Task DeleteAsync(Entity entity);
    }
}
