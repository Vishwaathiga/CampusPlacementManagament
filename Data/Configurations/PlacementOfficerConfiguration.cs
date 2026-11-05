using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class PlacementOfficerConfiguration
        : IEntityTypeConfiguration<PlacementOfficer>
    {
        public void Configure(EntityTypeBuilder<PlacementOfficer> builder)
        {
            builder.HasKey(p => p.PlacementOfficerId);

            builder.Property(p => p.Designation)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Status)
                .IsRequired();

            builder.HasIndex(p => p.UserId)
                .IsUnique();

            builder.HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<PlacementOfficer>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.College)
                .WithMany()
                .HasForeignKey(p => p.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Approver)
                .WithMany()
                .HasForeignKey(p => p.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}