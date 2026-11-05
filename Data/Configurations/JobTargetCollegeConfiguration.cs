using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class JobTargetCollegeConfiguration : IEntityTypeConfiguration<JobTargetCollege>
    {
        public void Configure(EntityTypeBuilder<JobTargetCollege> builder)
        {
            builder.HasKey(j => j.JobTargetCollegeId);

            builder.HasIndex(j => new { j.JobId, j.CollegeId })
                .IsUnique();

            builder.HasOne(j => j.Job)
                .WithMany(j => j.TargetColleges)
                .HasForeignKey(j => j.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(j => j.College)
                .WithMany()
                .HasForeignKey(j => j.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}