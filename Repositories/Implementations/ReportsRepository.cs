using CampusPlacementManagement.Data;
using CampusPlacementManagement.DTOs.Reports;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PlatformOverviewDto> GetPlatformOverviewAsync()
        {
            return new PlatformOverviewDto
            {
                TotalColleges = await _context.Colleges.CountAsync(),
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalStudents = await _context.Students.CountAsync(),
                TotalPlacementOfficers = await _context.PlacementOfficers.CountAsync(),
                TotalCompanyHRs = await _context.CompanyHRs.CountAsync(),
                TotalJobs = await _context.JobOpenings.CountAsync(),
                TotalApplications = await _context.JobApplications.CountAsync(),
                TotalSelectedStudents = await _context.JobApplications
                    .CountAsync(a => a.Status == ApplicationStatus.Selected)
            };
        }

        public async Task<CollegeOverviewDto?> GetCollegeOverviewAsync(int collegeId)
        {
            var college = await _context.Colleges
                .Where(c => c.CollegeId == collegeId)
                .Select(c => new CollegeOverviewDto
                {
                    CollegeId = c.CollegeId,
                    CollegeName = c.CollegeName,
                    TotalStudents = _context.Students.Count(s => s.CollegeId == collegeId),
                    TotalPlacementOfficers = _context.PlacementOfficers.Count(p => p.CollegeId == collegeId),
                    TotalApplications = _context.JobApplications
                        .Count(a => a.Student.CollegeId == collegeId),
                    TotalSelectedStudents = _context.JobApplications
                        .Count(a => a.Student.CollegeId == collegeId &&
                                    a.Status == ApplicationStatus.Selected)
                })
                .FirstOrDefaultAsync();

            return college;
        }

        public async Task<CompanyRecruitmentDto?> GetCompanyRecruitmentAsync(int companyId)
        {
            var company = await _context.Companies
                .Where(c => c.CompanyId == companyId)
                .Select(c => new CompanyRecruitmentDto
                {
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName,
                    TotalJobs = _context.JobOpenings.Count(j => j.CompanyId == companyId),
                    PublishedJobs = _context.JobOpenings.Count(j =>
                        j.CompanyId == companyId && j.Status == JobStatus.Published),
                    TotalApplications = _context.JobApplications
                        .Count(a => a.Job.CompanyId == companyId),
                    SelectedCandidates = _context.JobApplications
                        .Count(a => a.Job.CompanyId == companyId &&
                                    a.Status == ApplicationStatus.Selected)
                })
                .FirstOrDefaultAsync();

            return company;
        }

        public async Task<List<StudentPlacementDto>> GetStudentPlacementAsync(int collegeId)
        {
            return await _context.Students
                .Where(s => s.CollegeId == collegeId)
                .Select(s => new StudentPlacementDto
                {
                    StudentId = s.StudentId,
                    StudentName = s.User.FullName,
                    RegisterNumber = s.RegisterNumber,
                    Department = s.Department,
                    Degree = s.Degree,
                    GraduationYear = s.GraduationYear,
                    CGPA = s.CGPA,
                    TotalApplications = _context.JobApplications
                        .Count(a => a.StudentId == s.StudentId),
                    SelectedApplications = _context.JobApplications
                        .Count(a => a.StudentId == s.StudentId &&
                                    a.Status == ApplicationStatus.Selected)
                })
                .ToListAsync();
        }
    }
}