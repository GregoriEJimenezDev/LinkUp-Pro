using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class ReactionConfig : IEntityTypeConfiguration<Reaction>
    {
        public void Configure(EntityTypeBuilder<Reaction> builder)
        {
            #region Reaction
            builder.ToTable("Reactions");
            builder.HasKey(r => r.Id);
            #endregion

            #region index
            builder.HasIndex(r => new { r.PostId, r.UserId }).IsUnique();
            #endregion

        }
    }
}
