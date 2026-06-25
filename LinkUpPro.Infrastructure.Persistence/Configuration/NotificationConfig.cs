using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class NotificationConfig : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            #region Notification
            builder.ToTable("Notifications");
            builder.HasKey(n => n.Id);
            #endregion

            #region properties
            builder.Property(n => n.UserId).IsRequired();
            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
            builder.Property(n => n.Url).HasMaxLength(500);
            builder.Property(n => n.IsRead).HasDefaultValue(false);
            #endregion
        }
    }
}
