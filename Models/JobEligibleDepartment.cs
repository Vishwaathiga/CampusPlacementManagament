namespace CampusPlacementManagement.Models
{
    public class JobEligibleDepartment
    {
        public int JobEligibleDepartmentId { get; set; }
        public int JobId { get; set; }
        public string Department { get; set; } = string.Empty;

        public JobOpening Job { get; set; } = null!;
    }
}