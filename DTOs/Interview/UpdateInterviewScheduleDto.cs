using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.DTOs.Interview
{
    public class UpdateInterviewScheduleDto
    {
        public DateTime ScheduledAt { get; set; }

        public string? MeetingLink { get; set; }

        public string? Venue { get; set; }

        public InterviewStatus Status { get; set; }
    }
}