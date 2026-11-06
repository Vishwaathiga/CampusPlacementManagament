using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusPlacementManagement.Data.Configurations
{
    public class InterviewScheduleConfiguration
        : IEntityTypeConfiguration<InterviewSchedule>
    {
        public void Configure(EntityTypeBuilder<InterviewSchedule> builder)
        {
            builder.HasKey(i => i.InterviewScheduleId);

            builder.Property(i => i.MeetingLink)
                .HasMaxLength(500);

            builder.Property(i => i.Venue)
                .HasMaxLength(255);

            builder.Property(i => i.Status)
                .IsRequired();

            builder.HasOne(i => i.Application)
                .WithMany()
                .HasForeignKey(i => i.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.InterviewRound)
                .WithMany(r => r.InterviewSchedules)
                .HasForeignKey(i => i.InterviewRoundId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}