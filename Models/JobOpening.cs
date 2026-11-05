using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.Models
{
    public class JobOpening
    {
        public int JobId { get; set; }
        public int CompanyId { get; set; }
        public int CreatedByHRId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public JobType JobType { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal? SalaryPackage { get; set; }
        public decimal? MinimumCGPA { get; set; }
        public int? MaximumBacklogs { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public bool IsOpenToAllColleges { get; set; }
        public JobStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Company Company { get; set; } = null!;
        public ICollection<JobTargetCollege> TargetColleges { get; set; } = new List<JobTargetCollege>();
        public ICollection<JobEligibleDepartment> EligibleDepartments { get; set; } = new List<JobEligibleDepartment>();
        public ICollection<JobEligibleGraduationYear> EligibleGraduationYears { get; set; } = new List<JobEligibleGraduationYear>();
    }
}