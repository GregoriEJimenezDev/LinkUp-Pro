using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class AttackConfig : IEntityTypeConfiguration<Attack>
    {
        public void Configure(EntityTypeBuilder<Attack> builder)
        {
            #region Comment
            builder.ToTable("Attacks");
            builder.HasKey(a => a.Id);
            #endregion

            #region indexes
            builder.HasIndex(a => new { a.GameId, a.AttackerId, a.Row, a.Column }).IsUnique();
            #endregion

        }
    }
}
