using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.DTOs.Job
{
    public class UpdateJobDto
    {
        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public JobType JobType { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal? SalaryPackage { get; set; }
        public decimal? MinimumCGPA { get; set; }
        public int? MaximumBacklogs { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public bool IsOpenToAllColleges { get; set; }
        public List<string> EligibleDepartments { get; set; } = new();
        public List<int> EligibleGraduationYears { get; set; } = new();
    }
}