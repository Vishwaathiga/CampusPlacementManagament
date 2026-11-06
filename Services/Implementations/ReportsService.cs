using CampusPlacementManagement.DTOs.Reports;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services.Implementations
{
    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository _reportsRepository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly ICompanyHRRepository _companyHRRepository;

        public ReportsService(
            IReportsRepository reportsRepository,
            IPlacementOfficerRepository officerRepository,
            ICompanyHRRepository companyHRRepository)
        {
            _reportsRepository = reportsRepository;
            _officerRepository = officerRepository;
            _companyHRRepository = companyHRRepository;
        }

        public async Task<PlatformOverviewDto> GetPlatformOverviewAsync()
        {
            return await _reportsRepository.GetPlatformOverviewAsync();
        }

        public async Task<CollegeOverviewDto> GetCollegeOverviewAsync(string userId)
        {
            var officer = await _officerRepository.GetByUserIdAsync(userId);

            if (officer == null)
                throw new InvalidOperationException("Placement officer profile not found.");

            var report = await _reportsRepository.GetCollegeOverviewAsync(officer.CollegeId);

            if (report == null)
                throw new InvalidOperationException("College not found.");

            return report;
        }

        public async Task<CompanyRecruitmentDto> GetCompanyRecruitmentAsync(string userId)
        {
            var companyHR = await _companyHRRepository.GetByUserIdAsync(userId);

            if (companyHR == null)
                throw new InvalidOperationException("Company HR profile not found.");

            var report = await _reportsRepository
                .GetCompanyRecruitmentAsync(companyHR.CompanyId);

            if (report == null)
                throw new InvalidOperationException("Company not found.");

            return report;
        }

        public async Task<List<StudentPlacementDto>> GetStudentPlacementAsync(string userId)
        {
            var officer = await _officerRepository.GetByUserIdAsync(userId);

            if (officer == null)
                throw new InvalidOperationException("Placement officer profile not found.");

            return await _reportsRepository.GetStudentPlacementAsync(officer.CollegeId);
        }
    }
}