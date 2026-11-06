using CampusPlacementManagement.DTOs.Reports;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IReportsRepository
    {
        Task<PlatformOverviewDto> GetPlatformOverviewAsync();
        Task<CollegeOverviewDto?> GetCollegeOverviewAsync(int collegeId);
        Task<CompanyRecruitmentDto?> GetCompanyRecruitmentAsync(int companyId);
        Task<List<StudentPlacementDto>> GetStudentPlacementAsync(int collegeId);
    }
}