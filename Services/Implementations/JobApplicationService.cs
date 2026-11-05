
using CampusPlacementManagement.DTOs.JobApplication;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Implementations;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services.Implementations
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _applicationRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICollegeRepository _collegeRepository;

        public JobApplicationService(
            IJobApplicationRepository applicationRepository,
            IStudentRepository studentRepository,
            IJobRepository jobRepository,
            ICompanyHRRepository companyHRRepository,
            ICompanyRepository companyRepository,
            ICollegeRepository collegeRepository)
        {
            _applicationRepository = applicationRepository;
            _studentRepository = studentRepository;
            _jobRepository = jobRepository;
            _companyHRRepository = companyHRRepository;
            _companyRepository = companyRepository;
            _collegeRepository = collegeRepository;
        }

        public async Task<JobApplicationResponseDto> ApplyAsync(
            ApplyJobDto dto, string userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);

            if (student == null || student.Status != StudentStatus.Approved)
                throw new UnauthorizedAccessException("Approved student profile required.");

            var college = await _collegeRepository.GetByIdAsync(student.CollegeId);

            if (college == null || college.Status != CollegeStatus.Approved)
                throw new UnauthorizedAccessException("Your college is not approved.");

            var job = await _jobRepository.GetByIdAsync(dto.JobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            var company = await _companyRepository.GetByIdAsync(job.CompanyId);

            if (company == null || company.Status != CompanyStatus.Approved)
                throw new InvalidOperationException("Company is not approved.");

            if (job.Status != JobStatus.Published ||
                job.ApplicationDeadline <= DateTime.UtcNow)
                throw new InvalidOperationException(
                    "This job is not accepting applications.");

            if (!job.IsOpenToAllColleges &&
                !job.TargetColleges.Any(t => t.CollegeId == student.CollegeId))
                throw new InvalidOperationException(
                    "This job is not available to your college.");

            if (!IsEligible(job, student))
                throw new InvalidOperationException(
                    "You do not meet the eligibility criteria.");

            if (await _applicationRepository.ExistsAsync(
                    student.StudentId, job.JobId))
                throw new InvalidOperationException(
                    "You have already applied for this job.");

            if (dto.ResumeUrl != null && dto.ResumeUrl.Length > 500)
                throw new ArgumentException(
                    "Resume URL cannot exceed 500 characters.");

            var application = new JobApplication
            {
                JobId = job.JobId,
                StudentId = student.StudentId,
                AppliedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Applied,
                ResumeUrl = dto.ResumeUrl,
                LastUpdatedAt = DateTime.UtcNow
            };

            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                PreviousStatus = null,
                NewStatus = ApplicationStatus.Applied,
                UpdatedByUserId = userId,
                Remarks = "Application submitted."
            });

            await _applicationRepository.AddAsync(application);
            await _applicationRepository.SaveChangesAsync();

            application.Job = job;
            application.Student = student;

            return MapToDto(application);
        }

        public async Task<List<JobApplicationResponseDto>> GetMyApplicationsAsync(
            string userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);

            if (student == null)
                throw new UnauthorizedAccessException("Student profile not found.");

            var applications = await _applicationRepository
                .GetByStudentIdAsync(student.StudentId);

            return applications.Select(MapToDto).ToList();
        }

        public async Task<List<JobApplicationResponseDto>> GetApplicationsByJobAsync(
            int jobId, string userId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr == null || hr.CompanyId != job.CompanyId)
                throw new UnauthorizedAccessException(
                    "You cannot view applications for another company's job.");

            var applications = await _applicationRepository.GetByJobIdAsync(jobId);

            return applications.Select(MapToDto).ToList();
        }

        public async Task<JobApplicationResponseDto?> GetByIdAsync(
            int id, string userId)
        {
            var application = await _applicationRepository.GetByIdAsync(id);

            if (application == null)
                return null;

            var student = await _studentRepository.GetByUserIdAsync(userId);

            if (student != null && student.StudentId == application.StudentId)
                return MapToDto(application);

            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr != null && hr.CompanyId == application.Job.CompanyId)
                return MapToDto(application);

            throw new UnauthorizedAccessException(
                "You cannot view this application.");
        }

        public async Task<JobApplicationResponseDto> UpdateStatusAsync(
            int id, UpdateApplicationStatusDto dto, string userId)
        {
            var application = await _applicationRepository.GetByIdAsync(id);

            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr == null || hr.CompanyId != application.Job.CompanyId)
                throw new UnauthorizedAccessException(
                    "You cannot update another company's applications.");

            if (!IsAllowedStatus(dto.Status))
                throw new ArgumentException("Invalid application status.");

            if (application.Status == ApplicationStatus.Withdrawn ||
                application.Status == ApplicationStatus.Rejected ||
                application.Status == ApplicationStatus.Selected ||
                application.Status == ApplicationStatus.NotSelected)
                throw new InvalidOperationException(
                    "This application has reached a final status.");

            if (dto.Status == ApplicationStatus.Applied ||
                dto.Status == ApplicationStatus.Withdrawn)
                throw new InvalidOperationException(
                    "This status cannot be assigned by Company HR.");

            if (dto.Remarks != null && dto.Remarks.Length > 500)
                throw new ArgumentException("Remarks cannot exceed 500 characters.");

            var previousStatus = application.Status;

            application.Status = dto.Status;
            application.LastUpdatedAt = DateTime.UtcNow;

            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                PreviousStatus = previousStatus,
                NewStatus = dto.Status,
                UpdatedByUserId = userId,
                Remarks = dto.Remarks
            });

            await _applicationRepository.SaveChangesAsync();

            return MapToDto(application);
        }

        public async Task<List<ApplicationHistoryResponseDto>> GetHistoryAsync(
            int id, string userId)
        {
            var application = await _applicationRepository.GetByIdAsync(id);

            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            var student = await _studentRepository.GetByUserIdAsync(userId);
            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            bool isStudentOwner =
                student != null && student.StudentId == application.StudentId;

            bool isCompanyHR =
                hr != null && hr.CompanyId == application.Job.CompanyId;

            if (!isStudentOwner && !isCompanyHR)
                throw new UnauthorizedAccessException();

            return application.StatusHistory
                .OrderBy(h => h.UpdatedAt)
                .Select(h => new ApplicationHistoryResponseDto
                {
                    HistoryId = h.HistoryId,
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    UpdatedByUserId = h.UpdatedByUserId,
                    Remarks = h.Remarks,
                    UpdatedAt = h.UpdatedAt
                }).ToList();
        }

        public async Task WithdrawAsync(int id, string userId)
        {
            var application = await _applicationRepository.GetByIdAsync(id);

            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            var student = await _studentRepository.GetByUserIdAsync(userId);

            if (student == null || student.StudentId != application.StudentId)
                throw new UnauthorizedAccessException();

            var college = await _collegeRepository.GetByIdAsync(student.CollegeId);

            if (college == null || college.Status != CollegeStatus.Approved)
                throw new UnauthorizedAccessException(
                    "Your college is not approved.");

            if (application.Status != ApplicationStatus.Applied &&
                application.Status != ApplicationStatus.UnderReview)
                throw new InvalidOperationException(
                    "You cannot withdraw this application at its current stage.");

            var previousStatus = application.Status;

            application.Status = ApplicationStatus.Withdrawn;
            application.LastUpdatedAt = DateTime.UtcNow;

            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                PreviousStatus = previousStatus,
                NewStatus = ApplicationStatus.Withdrawn,
                UpdatedByUserId = userId,
                Remarks = "Application withdrawn by student."
            });

            await _applicationRepository.SaveChangesAsync();
        }

        private static bool IsEligible(JobOpening job, Student student)
        {
            bool departmentEligible =
                !job.EligibleDepartments.Any() ||
                job.EligibleDepartments.Any(d =>
                    d.Department.Equals(
                        student.Department,
                        StringComparison.OrdinalIgnoreCase));

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

            bool deadlineValid =
                job.ApplicationDeadline > DateTime.UtcNow;

            return departmentEligible &&
                   yearEligible &&
                   cgpaEligible &&
                   backlogEligible &&
                   deadlineValid;
        }

        private static bool IsAllowedStatus(ApplicationStatus status)
        {
            return status == ApplicationStatus.UnderReview ||
                   status == ApplicationStatus.Shortlisted ||
                   status == ApplicationStatus.Rejected ||
                   status == ApplicationStatus.InterviewScheduled ||
                   status == ApplicationStatus.Selected ||
                   status == ApplicationStatus.NotSelected;
        }

        private static JobApplicationResponseDto MapToDto(
            JobApplication application)
        {
            return new JobApplicationResponseDto
            {
                ApplicationId = application.ApplicationId,
                JobId = application.JobId,
                JobTitle = application.Job.JobTitle,
                StudentId = application.StudentId,
                StudentName = application.Student.User.FullName,
                RegisterNumber = application.Student.RegisterNumber,
                AppliedAt = application.AppliedAt,
                Status = application.Status,
                ResumeUrl = application.ResumeUrl,
                LastUpdatedAt = application.LastUpdatedAt
            };
        }
    }
}
