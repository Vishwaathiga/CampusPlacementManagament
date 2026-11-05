using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.StudentId);

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.RegisterNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(s => s.Department)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(s => s.Degree)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(s => s.CGPA)
                .HasPrecision(4, 2)
                .IsRequired();

            builder.Property(s => s.ActiveBacklogs)
                .IsRequired();

            builder.Property(s => s.ResumeUrl)
                .HasMaxLength(500);

            builder.HasIndex(s => new
            {
                s.CollegeId,
                s.RegisterNumber
            }).IsUnique();

            builder.HasIndex(s => s.UserId)
                .IsUnique();

            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.College)
                .WithMany()
                .HasForeignKey(s => s.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.PlacementOfficer)
                .WithMany()
                .HasForeignKey(s => s.PlacementOfficerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}