using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class ApplicationStatusHistoryConfiguration
        : IEntityTypeConfiguration<ApplicationStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ApplicationStatusHistory> builder)
        {
            builder.HasKey(h => h.HistoryId);

            builder.Property(h => h.UpdatedByUserId)
                .IsRequired();

            builder.Property(h => h.Remarks)
                .HasMaxLength(500);

            builder.HasOne(h => h.Application)
                .WithMany(a => a.StatusHistory)
                .HasForeignKey(h => h.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.UpdatedByUser)
                .WithMany()
                .HasForeignKey(h => h.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}