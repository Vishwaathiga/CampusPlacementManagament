
namespace CampusPlacementManagement.DTOs.RecruitmentDrive
{
    public class CreateRecruitmentDriveDto
    {
        public int JobId { get; set; }
        public int CollegeId { get; set; }
        public DateTime DriveDate { get; set; }
        public string? Venue { get; set; }
        public string? Description { get; set; }
    }
}
