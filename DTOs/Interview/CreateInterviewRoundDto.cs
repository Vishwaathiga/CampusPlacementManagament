namespace CampusPlacementManagement.DTOs.Interview
{
    public class CreateInterviewRoundDto
    {
        public string RoundName { get; set; } = string.Empty;

        public int RoundNumber { get; set; }

        public string? Description { get; set; }
    }
}