using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class JobOpeningConfiguration : IEntityTypeConfiguration<JobOpening>
    {
        public void Configure(EntityTypeBuilder<JobOpening> builder)
        {
            builder.HasKey(j => j.JobId);

            builder.Property(j => j.JobTitle)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(j => j.JobDescription)
                .IsRequired();

            builder.Property(j => j.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(j => j.SalaryPackage)
                .HasPrecision(12, 2);

            builder.Property(j => j.MinimumCGPA)
                .HasPrecision(4, 2);

            builder.HasOne(j => j.Company)
                .WithMany()
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<CompanyHR>()
                .WithMany()
                .HasForeignKey(j => j.CreatedByHRId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(j => j.CreatedByHRId);

            builder.HasIndex(j => j.Status);
        }
    }
}