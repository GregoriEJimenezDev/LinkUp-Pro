using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LinkUpPro.Infrastructure.Persistence.Context
{
    public class LinkUpProDbContext(DbContextOptions<LinkUpProDbContext> options) : DbContext(options)
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<BattleshipGame> BattleshipGames { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Ship> Ships { get; set; }
        public DbSet<ShipCell> ShipCells { get; set; }
        public DbSet<Attack> Attacks { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
