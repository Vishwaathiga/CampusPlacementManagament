namespace CampusPlacementManagement.DTOs.College
{
    public class CollegeRegisterDto
    {
        public string CollegeName { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;
        public string? UniversityName { get; set; }
        public string OfficialEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Website { get; set; }
    }
}