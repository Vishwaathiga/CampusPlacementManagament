using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class InterviewRoundConfiguration
        : IEntityTypeConfiguration<InterviewRound>
    {
        public void Configure(EntityTypeBuilder<InterviewRound> builder)
        {
            builder.HasKey(r => r.InterviewRoundId);

            builder.Property(r => r.RoundName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Description);

            builder.HasIndex(r => new { r.JobId, r.RoundNumber })
                .IsUnique();

            builder.HasOne(r => r.Job)
                .WithMany()
                .HasForeignKey(r => r.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}