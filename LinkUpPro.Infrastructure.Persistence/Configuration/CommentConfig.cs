using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class CommentConfig : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            #region Comment
            builder.ToTable("Comments");
            builder.HasKey(c => c.Id);
            #endregion

            #region properties
            builder.Property(c => c.Content).IsRequired().HasMaxLength(1000);
            builder.Property(c => c.UserId).IsRequired();
            #endregion

            #region conexions
            builder.HasOne(c => c.ParentComment)
                 .WithMany(c => c.Replies)
                 .HasForeignKey(c => c.ParentCommentId)
                 .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
