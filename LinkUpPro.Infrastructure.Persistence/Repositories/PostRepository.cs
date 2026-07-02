using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class PostRepository(LinkUpProDbContext context) : GenericRepository<Post>(context), IPostRepository
    {
        public async Task<IEnumerable<Post>> GetByFriendsAsync(IEnumerable<string> friendIds) 
        {
            var idlist = friendIds.ToList();
            if(!idlist.Any()) return [];

            var allpost= await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Comments).ThenInclude(c => c.Replies)
                .Include(p => p.Reactions)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();
            return allpost.Where(p => idlist.Contains(p.UserId!));
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(string userId) =>
            await _context.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .Include(p => p.Reactions)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();


        public async Task<IEnumerable<Post>> GetAllPostWithDetailsAsync() =>
            await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .Include(p => p.Reactions)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

        public async Task<IEnumerable<Post>> GetFeedPostsAsync(string userId, IEnumerable<string> friendIds, bool includeGlobalPublic, bool includeSelf)
        {
            var fIds = friendIds.ToList();
            
            var query = _context.Posts.Where(p => !p.IsDeleted);

            // Using standard OR logic compatible with EF Core translation
            query = query.Where(p => 
                (includeSelf && p.UserId == userId) || 
                (p.UserId != null && fIds.Contains(p.UserId) && p.Privacy != LinkUpPro.Core.Domain.Enum.PostPrivacy.OnlyMe) || 
                (includeGlobalPublic && p.Privacy == LinkUpPro.Core.Domain.Enum.PostPrivacy.Public)
            );

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .Include(p => p.Reactions)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}
