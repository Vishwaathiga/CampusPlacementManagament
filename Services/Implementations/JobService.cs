using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.Job;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services.Implementations
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IPlacementOfficerRepository _officerRepository;

        public JobService(
            IJobRepository jobRepository,
            ICompanyHRRepository companyHRRepository,
            ICompanyRepository companyRepository,
            ICollegeRepository collegeRepository,
            IStudentRepository studentRepository,
            IPlacementOfficerRepository officerRepository)
        {
            _jobRepository = jobRepository;
            _companyHRRepository = companyHRRepository;
            _companyRepository = companyRepository;
            _collegeRepository = collegeRepository;
            _studentRepository = studentRepository;
            _officerRepository = officerRepository;
        }

        public async Task<JobResponseDto> CreateAsync(CreateJobDto dto, string userId)
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr == null)
                throw new UnauthorizedAccessException("Company HR profile not found.");

            var company = await _companyRepository.GetByIdAsync(hr.CompanyId);

            if (company == null || company.Status != CompanyStatus.Approved)
                throw new InvalidOperationException("Only an approved company can create jobs.");

            ValidateJob(
                dto.JobTitle, dto.JobDescription, dto.Location,
                dto.ApplicationDeadline, dto.MinimumCGPA,
                dto.MaximumBacklogs, dto.SalaryPackage,
                dto.EligibleGraduationYears);

            var job = new JobOpening
            {
                CompanyId = hr.CompanyId,
                CreatedByHRId = hr.CompanyHRId,
                JobTitle = dto.JobTitle.Trim(),
                JobDescription = dto.JobDescription.Trim(),
                JobType = dto.JobType,
                Location = dto.Location.Trim(),
                SalaryPackage = dto.SalaryPackage,
                MinimumCGPA = dto.MinimumCGPA,
                MaximumBacklogs = dto.MaximumBacklogs,
                ApplicationDeadline = dto.ApplicationDeadline,
                IsOpenToAllColleges = dto.IsOpenToAllColleges,
                Status = JobStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                EligibleDepartments = dto.EligibleDepartments
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Select(d => new JobEligibleDepartment
                    {
                        Department = d.Trim()
                    }).ToList(),
                EligibleGraduationYears = dto.EligibleGraduationYears
                    .Distinct()
                    .Select(y => new JobEligibleGraduationYear
                    {
                        GraduationYear = y
                    }).ToList()
            };

            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            return MapToDto(job);
        }

        public async Task<List<JobResponseDto>> GetAllAsync(string userId, string role)
        {
            var jobs = await _jobRepository.GetAllAsync();

            if (role == UserRoles.SuperAdmin)
                return jobs.Select(MapToDto).ToList();

            if (role == UserRoles.CompanyHR)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(userId);

                if (hr == null)
                    throw new UnauthorizedAccessException();

                return jobs.Where(j => j.CompanyId == hr.CompanyId)
                    .Select(MapToDto).ToList();
            }

            var collegeId = await GetUserCollegeIdAsync(userId, role);

            return jobs.Where(j =>
                    j.Status == JobStatus.Published &&
                    IsVisibleToCollege(j, collegeId))
                .Select(MapToDto).ToList();
        }

        public async Task<JobResponseDto?> GetByIdAsync(int id, string userId, string role)
        {
            var job = await _jobRepository.GetByIdAsync(id);

            if (job == null)
                return null;

            if (role == UserRoles.SuperAdmin)
                return MapToDto(job);

            if (role == UserRoles.CompanyHR)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(userId);

                if (hr == null || hr.CompanyId != job.CompanyId)
                    throw new UnauthorizedAccessException();

                return MapToDto(job);
            }

            var collegeId = await GetUserCollegeIdAsync(userId, role);

            if (job.Status != JobStatus.Published ||
                !IsVisibleToCollege(job, collegeId))
                throw new UnauthorizedAccessException();

            return MapToDto(job);
        }

        public async Task<JobResponseDto> UpdateAsync(
            int id, UpdateJobDto dto, string userId)
        {
            var job = await GetOwnedJobAsync(id, userId);

            if (job.Status == JobStatus.Closed || job.Status == JobStatus.Cancelled)
                throw new InvalidOperationException("Closed or cancelled jobs cannot be updated.");

            ValidateJob(
                dto.JobTitle, dto.JobDescription, dto.Location,
                dto.ApplicationDeadline, dto.MinimumCGPA,
                dto.MaximumBacklogs, dto.SalaryPackage,
                dto.EligibleGraduationYears);

            job.JobTitle = dto.JobTitle.Trim();
            job.JobDescription = dto.JobDescription.Trim();
            job.JobType = dto.JobType;
            job.Location = dto.Location.Trim();
            job.SalaryPackage = dto.SalaryPackage;
            job.MinimumCGPA = dto.MinimumCGPA;
            job.MaximumBacklogs = dto.MaximumBacklogs;
            job.ApplicationDeadline = dto.ApplicationDeadline;
            job.IsOpenToAllColleges = dto.IsOpenToAllColleges;

            job.EligibleDepartments.Clear();
            foreach (var department in dto.EligibleDepartments
                         .Where(d => !string.IsNullOrWhiteSpace(d)).Distinct())
            {
                job.EligibleDepartments.Add(new JobEligibleDepartment
                {
                    Department = department.Trim()
                });
            }

            job.EligibleGraduationYears.Clear();
            foreach (var year in dto.EligibleGraduationYears.Distinct())
            {
                job.EligibleGraduationYears.Add(new JobEligibleGraduationYear
                {
                    GraduationYear = year
                });
            }

            await _jobRepository.SaveChangesAsync();
            return MapToDto(job);
        }

        public async Task<JobResponseDto> PublishAsync(int id, string userId)
        {
            var job = await GetOwnedJobAsync(id, userId);

            var company = await _companyRepository.GetByIdAsync(job.CompanyId);

            if (company == null || company.Status != CompanyStatus.Approved)
                throw new InvalidOperationException("The company must be approved.");

            if (job.Status != JobStatus.Draft)
                throw new InvalidOperationException("Only draft jobs can be published.");

            if (job.ApplicationDeadline <= DateTime.UtcNow)
                throw new InvalidOperationException("Application deadline must be in the future.");

            if (!job.IsOpenToAllColleges && !job.TargetColleges.Any())
                throw new InvalidOperationException("Select target colleges before publishing.");

            job.Status = JobStatus.Published;

            await _jobRepository.SaveChangesAsync();
            return MapToDto(job);
        }

        public async Task<JobResponseDto> CloseAsync(int id, string userId)
        {
            var job = await GetOwnedJobAsync(id, userId);

            if (job.Status != JobStatus.Published)
                throw new InvalidOperationException("Only published jobs can be closed.");

            job.Status = JobStatus.Closed;

            await _jobRepository.SaveChangesAsync();
            return MapToDto(job);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var job = await GetOwnedJobAsync(id, userId);

            if (job.Status != JobStatus.Draft)
                throw new InvalidOperationException("Only draft jobs can be deleted.");

            _jobRepository.Delete(job);
            await _jobRepository.SaveChangesAsync();
        }

        public async Task<List<JobResponseDto>> GetEligibleJobsAsync(string userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);

            if (student == null || student.Status != StudentStatus.Approved)
                throw new UnauthorizedAccessException("Approved student profile required.");

            var college = await _collegeRepository.GetByIdAsync(student.CollegeId);

            if (college == null || college.Status != CollegeStatus.Approved)
                throw new UnauthorizedAccessException("Your college is not approved.");

            var jobs = await _jobRepository.GetAllAsync();

            return jobs.Where(j =>
                    j.Status == JobStatus.Published &&
                    IsVisibleToCollege(j, student.CollegeId) &&
                    IsEligible(j, student))
                .Select(MapToDto).ToList();
        }

        public async Task<bool> CheckEligibilityAsync(int jobId, string userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);

            if (student == null || student.Status != StudentStatus.Approved)
                throw new UnauthorizedAccessException("Approved student profile required.");

            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            return job.Status == JobStatus.Published &&
                   job.ApplicationDeadline > DateTime.UtcNow &&
                   IsVisibleToCollege(job, student.CollegeId) &&
                   IsEligible(job, student);
        }

        public async Task<List<int>> GetTargetCollegesAsync(int jobId, string userId)
        {
            var job = await GetOwnedJobAsync(jobId, userId);
            return job.TargetColleges.Select(t => t.CollegeId).ToList();
        }

        public async Task AddTargetCollegesAsync(
            int jobId, TargetCollegesDto dto, string userId)
        {
            var job = await GetOwnedJobAsync(jobId, userId);

            if (job.Status != JobStatus.Draft)
                throw new InvalidOperationException("Target colleges can only be changed for draft jobs.");

            if (job.IsOpenToAllColleges)
                throw new InvalidOperationException("This job is open to all colleges.");

            foreach (var collegeId in dto.CollegeIds.Distinct())
            {
                var college = await _collegeRepository.GetByIdAsync(collegeId);

                if (college == null || college.Status != CollegeStatus.Approved)
                    throw new InvalidOperationException($"College ID {collegeId} is not approved.");

                if (!job.TargetColleges.Any(t => t.CollegeId == collegeId))
                {
                    job.TargetColleges.Add(new JobTargetCollege
                    {
                        CollegeId = collegeId
                    });
                }
            }

            await _jobRepository.SaveChangesAsync();
        }

        public async Task RemoveTargetCollegeAsync(
            int jobId, int collegeId, string userId)
        {
            var job = await GetOwnedJobAsync(jobId, userId);

            if (job.Status != JobStatus.Draft)
                throw new InvalidOperationException("Target colleges can only be changed for draft jobs.");

            var target = job.TargetColleges.FirstOrDefault(t => t.CollegeId == collegeId);

            if (target == null)
                throw new KeyNotFoundException("Target college not found.");

            job.TargetColleges.Remove(target);
            await _jobRepository.SaveChangesAsync();
        }

        private async Task<JobOpening> GetOwnedJobAsync(int id, string userId)
        {
            var job = await _jobRepository.GetByIdAsync(id);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr == null || hr.CompanyId != job.CompanyId)
                throw new UnauthorizedAccessException("You cannot manage another company's job.");

            return job;
        }

        private async Task<int> GetUserCollegeIdAsync(string userId, string role)
        {
            if (role == UserRoles.Student)
            {
                var student = await _studentRepository.GetByUserIdAsync(userId);

                if (student == null || student.Status != StudentStatus.Approved)
                    throw new UnauthorizedAccessException();

                return student.CollegeId;
            }

            if (role == UserRoles.PlacementOfficer)
            {
                var officer = await _officerRepository.GetByUserIdAsync(userId);

                if (officer == null ||
                    officer.Status != PlacementOfficerStatus.Approved ||
                    officer.College.Status != CollegeStatus.Approved)
                    throw new UnauthorizedAccessException();

                return officer.CollegeId;
            }

            throw new UnauthorizedAccessException();
        }

        private static bool IsVisibleToCollege(JobOpening job, int collegeId)
        {
            return job.IsOpenToAllColleges ||
                   job.TargetColleges.Any(t => t.CollegeId == collegeId);
        }

        private static bool IsEligible(JobOpening job, Student student)
        {
            bool departmentEligible =
                !job.EligibleDepartments.Any() ||
                job.EligibleDepartments.Any(d =>
                    d.Department.Equals(student.Department, StringComparison.OrdinalIgnoreCase));

            bool yearEligible =
                !job.EligibleGraduationYears.Any() ||
                job.EligibleGraduationYears.Any(y =>
                    y.GraduationYear == student.GraduationYear);

            bool cgpaEligible =
                !job.MinimumCGPA.HasValue ||
                student.CGPA >= job.MinimumCGPA.Value;

            bool backlogEligible =
                !job.MaximumBacklogs.HasValue ||
                student.ActiveBacklogs <= job.MaximumBacklogs.Value;

            return departmentEligible && yearEligible &&
                   cgpaEligible && backlogEligible &&
                   job.ApplicationDeadline > DateTime.UtcNow;
        }

        private static void ValidateJob(
            string title, string description, string location,
            DateTime deadline, decimal? cgpa, int? backlogs,
            decimal? salary, List<int> years)
        {
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Required job details cannot be empty.");

            if (deadline <= DateTime.UtcNow)
                throw new ArgumentException("Application deadline must be in the future.");

            if (cgpa.HasValue && (cgpa < 0 || cgpa > 10))
                throw new ArgumentException("Minimum CGPA must be between 0 and 10.");

            if (backlogs.HasValue && backlogs < 0)
                throw new ArgumentException("Maximum backlogs cannot be negative.");

            if (salary.HasValue && salary < 0)
                throw new ArgumentException("Salary cannot be negative.");

            if (years.Any(y => y < 2000 || y > 2100))
                throw new ArgumentException("Enter valid graduation years.");
        }

        private static JobResponseDto MapToDto(JobOpening job)
        {
            return new JobResponseDto
            {
                JobId = job.JobId,
                CompanyId = job.CompanyId,
                JobTitle = job.JobTitle,
                JobDescription = job.JobDescription,
                JobType = job.JobType,
                Location = job.Location,
                SalaryPackage = job.SalaryPackage,
                MinimumCGPA = job.MinimumCGPA,
                MaximumBacklogs = job.MaximumBacklogs,
                ApplicationDeadline = job.ApplicationDeadline,
                IsOpenToAllColleges = job.IsOpenToAllColleges,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                EligibleDepartments = job.EligibleDepartments
                    .Select(d => d.Department).ToList(),
                EligibleGraduationYears = job.EligibleGraduationYears
                    .Select(y => y.GraduationYear).ToList(),
                TargetCollegeIds = job.TargetColleges
                    .Select(t => t.CollegeId).ToList()
            };
        }
    }
}