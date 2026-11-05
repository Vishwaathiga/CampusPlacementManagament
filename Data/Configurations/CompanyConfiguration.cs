using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(c => c.CompanyId);

            builder.Property(c => c.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.CompanyEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(c => c.Website)
                .HasMaxLength(255);

            builder.Property(c => c.Industry)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.Description);

            builder.Property(c => c.Status)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();
        }
    }
}