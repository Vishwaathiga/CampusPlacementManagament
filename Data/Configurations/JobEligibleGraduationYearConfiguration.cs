using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class JobEligibleGraduationYearConfiguration : IEntityTypeConfiguration<JobEligibleGraduationYear>
    {
        public void Configure(EntityTypeBuilder<JobEligibleGraduationYear> builder)
        {
            builder.HasKey(y => y.JobEligibleGraduationYearId);

            builder.HasIndex(y => new { y.JobId, y.GraduationYear })
                .IsUnique();

            builder.HasOne(y => y.Job)
                .WithMany(j => j.EligibleGraduationYears)
                .HasForeignKey(y => y.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}