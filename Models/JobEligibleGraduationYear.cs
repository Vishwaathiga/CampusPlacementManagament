namespace CampusPlacementManagement.Models
{
    public class JobEligibleGraduationYear
    {
        public int JobEligibleGraduationYearId { get; set; }
        public int JobId { get; set; }
        public int GraduationYear { get; set; }

        public JobOpening Job { get; set; } = null!;
    }
}