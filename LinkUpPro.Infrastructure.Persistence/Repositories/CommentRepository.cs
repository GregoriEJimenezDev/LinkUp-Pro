using Microsoft.EntityFrameworkCore;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class CommentRepository(LinkUpProDbContext context) : GenericRepository<Comment>(context), ICommentRepository
    {
        public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId) =>
            await _context.Comments.Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAt)
            .Include(c => c.Replies)
            .AsNoTracking()
            .ToListAsync();

        public async Task<IEnumerable<Comment>> GetRepliesByCommentIdAsync(int commentId) =>
            await _context.Comments.Where(c => c.ParentCommentId == commentId)
            .OrderBy(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
