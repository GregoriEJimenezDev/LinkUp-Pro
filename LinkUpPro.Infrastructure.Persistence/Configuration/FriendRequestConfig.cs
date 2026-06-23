using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.Configuration
{
    public class FriendRequestConfig : IEntityTypeConfiguration<FriendRequest>
    {
        public void Configure(EntityTypeBuilder<FriendRequest> builder)
        {
            #region FriendRequest
            builder.ToTable("FriendRequests");
            builder.HasKey(f => f.Id);
            #endregion

            #region properties
            builder.Property(f => f.SenderId).IsRequired();
            builder.Property(f => f.ReceiverId).IsRequired();
            builder.Property(f => f.Status).HasConversion<string>();
            #endregion

        }
    }
}
