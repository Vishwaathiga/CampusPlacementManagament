namespace CampusPlacementManagement.DTOs.Student
{
    public class StudentUpdateDto
    {
        public string? PhoneNumber { get; set; }

        public string? Department { get; set; }

        public string? Degree { get; set; }

        public int? GraduationYear { get; set; }

        public decimal? CGPA { get; set; }

        public int? ActiveBacklogs { get; set; }

        public string? ResumeUrl { get; set; }
    }
}