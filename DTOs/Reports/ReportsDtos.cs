namespace CampusPlacementManagement.DTOs.Reports
{
    public class PlatformOverviewDto
    {
        public int TotalColleges { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalStudents { get; set; }
        public int TotalPlacementOfficers { get; set; }
        public int TotalCompanyHRs { get; set; }
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int TotalSelectedStudents { get; set; }
    }

    public class CollegeOverviewDto
    {
        public int CollegeId { get; set; }
        public string CollegeName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalPlacementOfficers { get; set; }
        public int TotalApplications { get; set; }
        public int TotalSelectedStudents { get; set; }
    }

    public class CompanyRecruitmentDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int TotalJobs { get; set; }
        public int PublishedJobs { get; set; }
        public int TotalApplications { get; set; }
        public int SelectedCandidates { get; set; }
    }

    public class StudentPlacementDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RegisterNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public decimal CGPA { get; set; }
        public int TotalApplications { get; set; }
        public int SelectedApplications { get; set; }
    }
}