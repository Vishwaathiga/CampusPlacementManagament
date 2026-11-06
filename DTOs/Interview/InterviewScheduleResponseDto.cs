using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.DTOs.Interview
{
    public class InterviewScheduleResponseDto
    {
        public int InterviewScheduleId { get; set; }

        public int ApplicationId { get; set; }

        public int InterviewRoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public DateTime ScheduledAt { get; set; }

        public string? MeetingLink { get; set; }

        public string? Venue { get; set; }

        public InterviewStatus Status { get; set; }

        public string? Feedback { get; set; }
    }
}