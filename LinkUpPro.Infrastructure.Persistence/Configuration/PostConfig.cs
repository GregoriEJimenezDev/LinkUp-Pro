using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class PostConfig : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            #region post
            builder.ToTable("Posts");
            builder.HasKey(p => p.Id);
            #endregion

            #region properties
            builder.Property(p => p.Content).IsRequired().HasMaxLength(2000);
            builder.Property(p => p.UserId).IsRequired();
            builder.HasQueryFilter(p => !p.IsDeleted);
            #endregion

            #region conexions
            builder.HasMany(p => p.Comments)
                 .WithOne(c => c.Post)
                 .HasForeignKey(c => c.PostId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Reactions)
                 .WithOne(r => r.Posts)
                 .HasForeignKey(r => r.PostId)
                 .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
