using CampusPlacementManagement.DTOs.Reports;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IReportsService
    {
        Task<PlatformOverviewDto> GetPlatformOverviewAsync();
        Task<CollegeOverviewDto> GetCollegeOverviewAsync(string userId);
        Task<CompanyRecruitmentDto> GetCompanyRecruitmentAsync(string userId);
        Task<List<StudentPlacementDto>> GetStudentPlacementAsync(string userId);
    }
}