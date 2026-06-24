using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class BattleshipGameConfig : IEntityTypeConfiguration<BattleshipGame>
    {
        public void Configure(EntityTypeBuilder<BattleshipGame> builder)
        {
            #region Comment
            builder.ToTable("BattleshipGames");
            builder.HasKey(g => g.Id);
            #endregion

            #region properties
            builder.Property(g => g.Status).HasConversion<string>();
            builder.Property(g => g.ConcurrencyStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
            #endregion

            #region conexions
            builder.HasMany(g => g.Ships)
                 .WithOne(s => s.Game)
                 .HasForeignKey(s => s.GameId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(g => g.Attacks)
                 .WithOne(a => a.Game)
                 .HasForeignKey(a => a.GameId)
                 .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
