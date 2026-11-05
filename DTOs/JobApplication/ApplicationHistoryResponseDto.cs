using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.DTOs.JobApplication
{
    public class ApplicationHistoryResponseDto
    {
        public int HistoryId { get; set; }
        public ApplicationStatus? PreviousStatus { get; set; }
        public ApplicationStatus NewStatus { get; set; }
        public string UpdatedByUserId { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}