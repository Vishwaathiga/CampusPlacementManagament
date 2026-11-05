namespace CampusPlacementManagement.DTOs.Student
{
    public class BulkStudentEnrollResultDto
    {
        public int SuccessfulEnrollments { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}