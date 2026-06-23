using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class FriendshipConfig : IEntityTypeConfiguration<Friendship>
    {
        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            #region Friendship
            builder.ToTable("Friendships");
            builder.HasKey(f => f.Id);
            builder.HasIndex(f => new { f.FirstUserId, f.SecondUserId }).IsUnique();
            #endregion

        }
    }
}