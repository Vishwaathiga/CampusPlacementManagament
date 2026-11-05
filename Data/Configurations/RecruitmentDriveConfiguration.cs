
using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class RecruitmentDriveConfiguration
        : IEntityTypeConfiguration<RecruitmentDrive>
    {
        public void Configure(EntityTypeBuilder<RecruitmentDrive> builder)
        {
            builder.HasKey(d => d.DriveId);

            builder.Property(d => d.Venue)
                .HasMaxLength(255);

            builder.Property(d => d.Status)
                .IsRequired();

            builder.HasOne(d => d.Job)
                .WithMany()
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.College)
                .WithMany()
                .HasForeignKey(d => d.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
