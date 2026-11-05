
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.RecruitmentDrive;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services.Implementations
{
    public class RecruitmentDriveService : IRecruitmentDriveService
    {
        private readonly IRecruitmentDriveRepository _driveRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly IStudentRepository _studentRepository;

        public RecruitmentDriveService(
            IRecruitmentDriveRepository driveRepository,
            IJobRepository jobRepository,
            ICollegeRepository collegeRepository,
            ICompanyHRRepository companyHRRepository,
            ICompanyRepository companyRepository,
            IPlacementOfficerRepository officerRepository,
            IStudentRepository studentRepository)
        {
            _driveRepository = driveRepository;
            _jobRepository = jobRepository;
            _collegeRepository = collegeRepository;
            _companyHRRepository = companyHRRepository;
            _companyRepository = companyRepository;
            _officerRepository = officerRepository;
            _studentRepository = studentRepository;
        }

        public async Task<RecruitmentDriveResponseDto> CreateAsync(
            CreateRecruitmentDriveDto dto, string userId)
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr == null)
                throw new UnauthorizedAccessException(
                    "Company HR profile not found.");

            var job = await _jobRepository.GetByIdAsync(dto.JobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (job.CompanyId != hr.CompanyId)
                throw new UnauthorizedAccessException(
                    "You cannot create a drive for another company's job.");

            var company = await _companyRepository.GetByIdAsync(job.CompanyId);

            if (company == null || company.Status != CompanyStatus.Approved)
                throw new InvalidOperationException(
                    "Company is not approved.");

            if (job.Status != JobStatus.Published)
                throw new InvalidOperationException(
                    "A recruitment drive can only be created for a published job.");

            var college = await _collegeRepository.GetByIdAsync(dto.CollegeId);

            if (college == null)
                throw new KeyNotFoundException("College not found.");

            if (college.Status != CollegeStatus.Approved)
                throw new InvalidOperationException(
                    "College is not approved.");

            if (!job.IsOpenToAllColleges &&
                !job.TargetColleges.Any(t => t.CollegeId == dto.CollegeId))
            {
                throw new InvalidOperationException(
                    "This job is not targeted to the selected college.");
            }

            if (dto.DriveDate <= DateTime.UtcNow)
                throw new ArgumentException(
                    "Drive date must be in the future.");

            if (dto.Venue != null && dto.Venue.Length > 255)
                throw new ArgumentException(
                    "Venue cannot exceed 255 characters.");

            var drive = new RecruitmentDrive
            {
                JobId = dto.JobId,
                CollegeId = dto.CollegeId,
                DriveDate = dto.DriveDate,
                Venue = dto.Venue,
                Description = dto.Description,
                Status = RecruitmentDriveStatus.Scheduled
            };

            await _driveRepository.AddAsync(drive);
            await _driveRepository.SaveChangesAsync();

            var savedDrive = await _driveRepository.GetByIdAsync(drive.DriveId);

            return MapToDto(savedDrive!);
        }

        public async Task<List<RecruitmentDriveResponseDto>> GetAllAsync(
            string userId, string role)
        {
            var drives = await _driveRepository.GetAllAsync();

            if (role == UserRoles.SuperAdmin)
                return drives.Select(MapToDto).ToList();

            if (role == UserRoles.CompanyHR)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(userId);

                if (hr == null)
                    throw new UnauthorizedAccessException();

                return drives
                    .Where(d => d.Job.CompanyId == hr.CompanyId)
                    .Select(MapToDto)
                    .ToList();
            }

            int collegeId;

            if (role == UserRoles.PlacementOfficer)
            {
                var officer = await _officerRepository.GetByUserIdAsync(userId);

                if (officer == null ||
                    officer.Status != PlacementOfficerStatus.Approved)
                    throw new UnauthorizedAccessException();

                if (officer.College.Status != CollegeStatus.Approved)
                    throw new UnauthorizedAccessException(
                        "Your college is not approved.");

                collegeId = officer.CollegeId;
            }
            else if (role == UserRoles.Student)
            {
                var student = await _studentRepository.GetByUserIdAsync(userId);

                if (student == null || student.Status != StudentStatus.Approved)
                    throw new UnauthorizedAccessException();

                var college = await _collegeRepository.GetByIdAsync(
                    student.CollegeId);

                if (college == null ||
                    college.Status != CollegeStatus.Approved)
                    throw new UnauthorizedAccessException(
                        "Your college is not approved.");

                collegeId = student.CollegeId;
            }
            else
            {
                throw new UnauthorizedAccessException();
            }

            return drives
                .Where(d => d.CollegeId == collegeId)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<RecruitmentDriveResponseDto?> GetByIdAsync(
            int id, string userId, string role)
        {
            var drive = await _driveRepository.GetByIdAsync(id);

            if (drive == null)
                return null;

            bool hasAccess = false;

            if (role == UserRoles.SuperAdmin)
            {
                hasAccess = true;
            }
            else if (role == UserRoles.CompanyHR)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(userId);

                hasAccess = hr != null &&
                            hr.CompanyId == drive.Job.CompanyId;
            }
            else if (role == UserRoles.PlacementOfficer)
            {
                var officer = await _officerRepository.GetByUserIdAsync(userId);

                hasAccess = officer != null &&
                            officer.Status == PlacementOfficerStatus.Approved &&
                            officer.College.Status == CollegeStatus.Approved &&
                            officer.CollegeId == drive.CollegeId;
            }
            else if (role == UserRoles.Student)
            {
                var student = await _studentRepository.GetByUserIdAsync(userId);

                if (student != null &&
                    student.Status == StudentStatus.Approved)
                {
                    var college = await _collegeRepository.GetByIdAsync(
                        student.CollegeId);

                    hasAccess = college != null &&
                                college.Status == CollegeStatus.Approved &&
                                student.CollegeId == drive.CollegeId;
                }
            }

            if (!hasAccess)
                throw new UnauthorizedAccessException();

            return MapToDto(drive);
        }

        public async Task<RecruitmentDriveResponseDto> UpdateAsync(
            int id, UpdateRecruitmentDriveDto dto, string userId, string role)
        {
            var drive = await _driveRepository.GetByIdAsync(id);

            if (drive == null)
                throw new KeyNotFoundException("Recruitment drive not found.");

            if (role != UserRoles.SuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(userId);

                if (hr == null || hr.CompanyId != drive.Job.CompanyId)
                    throw new UnauthorizedAccessException(
                        "You cannot update another company's recruitment drive.");
            }

            if (drive.Status != RecruitmentDriveStatus.Scheduled)
                throw new InvalidOperationException(
                    "Only scheduled drives can be updated.");

            if (dto.DriveDate <= DateTime.UtcNow)
                throw new ArgumentException(
                    "Drive date must be in the future.");

            if (dto.Venue != null && dto.Venue.Length > 255)
                throw new ArgumentException(
                    "Venue cannot exceed 255 characters.");

            drive.DriveDate = dto.DriveDate;
            drive.Venue = dto.Venue;
            drive.Description = dto.Description;

            await _driveRepository.SaveChangesAsync();

            return MapToDto(drive);
        }

        public async Task CancelAsync(int id, string userId, string role)
        {
            var drive = await _driveRepository.GetByIdAsync(id);

            if (drive == null)
                throw new KeyNotFoundException("Recruitment drive not found.");

            if (role != UserRoles.SuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(userId);

                if (hr == null || hr.CompanyId != drive.Job.CompanyId)
                    throw new UnauthorizedAccessException(
                        "You cannot cancel another company's recruitment drive.");
            }

            if (drive.Status == RecruitmentDriveStatus.Completed ||
                drive.Status == RecruitmentDriveStatus.Cancelled)
                throw new InvalidOperationException(
                    "This drive cannot be cancelled.");

            drive.Status = RecruitmentDriveStatus.Cancelled;

            await _driveRepository.SaveChangesAsync();
        }

        private static RecruitmentDriveResponseDto MapToDto(
            RecruitmentDrive drive)
        {
            return new RecruitmentDriveResponseDto
            {
                DriveId = drive.DriveId,
                JobId = drive.JobId,
                JobTitle = drive.Job.JobTitle,
                CollegeId = drive.CollegeId,
                CollegeName = drive.College.CollegeName,
                DriveDate = drive.DriveDate,
                Venue = drive.Venue,
                Description = drive.Description,
                Status = drive.Status
            };
        }
    }
}
