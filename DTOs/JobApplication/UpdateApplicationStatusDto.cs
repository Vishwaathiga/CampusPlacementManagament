using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.DTOs.JobApplication
{
    public class UpdateApplicationStatusDto
    {
        public ApplicationStatus Status { get; set; }
        public string? Remarks { get; set; }
    }
}