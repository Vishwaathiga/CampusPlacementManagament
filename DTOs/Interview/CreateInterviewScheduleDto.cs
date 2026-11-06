namespace CampusPlacementManagement.DTOs.Interview
{
    public class CreateInterviewScheduleDto
    {
        public int InterviewRoundId { get; set; }

        public DateTime ScheduledAt { get; set; }

        public string? MeetingLink { get; set; }

        public string? Venue { get; set; }
    }
}