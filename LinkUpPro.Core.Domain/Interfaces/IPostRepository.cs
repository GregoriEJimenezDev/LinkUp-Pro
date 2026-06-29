using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<IEnumerable<Post>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Post>> GetByFriendsAsync(IEnumerable<string> friendIds);
        Task<IEnumerable<Post>> GetAllPostWithDetailsAsync();
    }
}
