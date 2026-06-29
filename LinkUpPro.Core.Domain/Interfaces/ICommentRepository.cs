using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
        Task<IEnumerable<Comment>> GetRepliesByCommentIdAsync(int commentId);
    }
}
