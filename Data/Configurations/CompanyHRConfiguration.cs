using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class CompanyHRConfiguration : IEntityTypeConfiguration<CompanyHR>
    {
        public void Configure(EntityTypeBuilder<CompanyHR> builder)
        {
            builder.HasKey(h => h.CompanyHRId);

            builder.Property(h => h.UserId)
                .IsRequired();

            builder.Property(h => h.Designation)
                .HasMaxLength(100);

            builder.HasIndex(h => h.UserId)
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Company>()
                .WithMany()
                .HasForeignKey(h => h.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(h => h.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}