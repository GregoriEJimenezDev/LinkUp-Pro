using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IReactionRepository : IGenericRepository<Reaction>
    {
        Task<Reaction?> GetByPostAndUserAsync(int postId, string userId);
        Task<IEnumerable<Reaction>> GetByPostIdAsync(int postId);
    }
}
