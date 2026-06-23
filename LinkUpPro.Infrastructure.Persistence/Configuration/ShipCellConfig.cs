using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class ShipCellConfig : IEntityTypeConfiguration<ShipCell>
    {
        public void Configure(EntityTypeBuilder<ShipCell> builder)
        {
            #region Comment
            builder.ToTable("ShipCells");
            builder.HasKey(c => c.Id);
            #endregion

            #region indexes
            builder.HasIndex(c => new { c.ShipId, c.Row, c.Column }).IsUnique();
            #endregion

        }
    }
}
