using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.Models
{
    public class JobApplication
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public int StudentId { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
        public string? ResumeUrl { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public JobOpening Job { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public ICollection<ApplicationStatusHistory> StatusHistory { get; set; }
            = new List<ApplicationStatusHistory>();
    }
}