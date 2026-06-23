using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class ShipConfig : IEntityTypeConfiguration<Ship>
    {
        public void Configure(EntityTypeBuilder<Ship> builder)
        {
            #region Comment
            builder.ToTable("Ships");
            builder.HasKey(s => s.Id);
            #endregion

            #region properties
            builder.Property(s => s.ShipType).HasConversion<string>();
            #endregion

            #region conexions
            builder.HasMany(s => s.Cells)
                 .WithOne(c => c.Ship)
                 .HasForeignKey(c => c.ShipId)
                 .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
