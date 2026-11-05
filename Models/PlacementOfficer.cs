using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.Models
{
    public class PlacementOfficer
    {
        public int PlacementOfficerId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int CollegeId { get; set; }

        public string Designation { get; set; } = string.Empty;

        public PlacementOfficerStatus Status { get; set; }

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public ApplicationUser User { get; set; } = null!;

        public College College { get; set; } = null!;

        public ApplicationUser? Approver { get; set; }
    }
}