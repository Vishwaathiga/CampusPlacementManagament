using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.Interview;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services.Implementations
{
    public class InterviewRoundService : IInterviewRoundService
    {
        private readonly IInterviewRoundRepository _roundRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly IJobApplicationRepository _applicationRepository;
        private readonly IPlacementOfficerRepository _officerRepository;

        public InterviewRoundService(
            IInterviewRoundRepository roundRepository,
            IJobRepository jobRepository,
            ICompanyHRRepository companyHRRepository,
            IJobApplicationRepository applicationRepository,
            IPlacementOfficerRepository officerRepository)
        {
            _roundRepository = roundRepository;
            _jobRepository = jobRepository;
            _companyHRRepository = companyHRRepository;
            _applicationRepository = applicationRepository;
            _officerRepository = officerRepository;
        }

        public async Task<InterviewRoundResponseDto> CreateAsync(
            int jobId, CreateInterviewRoundDto dto, string userId)
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr == null)
                throw new UnauthorizedAccessException("Company HR profile not found.");

            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (job.CompanyId != hr.CompanyId)
                throw new UnauthorizedAccessException(
                    "You cannot manage interview rounds for this job.");

            if (string.IsNullOrWhiteSpace(dto.RoundName))
                throw new InvalidOperationException("Round name is required.");

            if (dto.RoundNumber <= 0)
                throw new InvalidOperationException("Round number must be greater than zero.");

            if (await _roundRepository.ExistsRoundNumberAsync(jobId, dto.RoundNumber))
                throw new InvalidOperationException(
                    "This round number already exists for the job.");

            var round = new InterviewRound
            {
                JobId = jobId,
                RoundName = dto.RoundName.Trim(),
                RoundNumber = dto.RoundNumber,
                Description = dto.Description
            };

            await _roundRepository.AddAsync(round);
            await _roundRepository.SaveChangesAsync();

            return new InterviewRoundResponseDto
            {
                InterviewRoundId = round.InterviewRoundId,
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                RoundName = round.RoundName,
                RoundNumber = round.RoundNumber,
                Description = round.Description
            };
        }

        public async Task<List<InterviewRoundResponseDto>> GetByJobIdAsync(
            int jobId, string userId, string role)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (role == UserRoles.CompanyHR)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(userId);

                if (hr == null || hr.CompanyId != job.CompanyId)
                    throw new UnauthorizedAccessException(
                        "You cannot view these interview rounds.");
            }
            else if (role == UserRoles.Student)
            {
                var applications = await _applicationRepository.GetByJobIdAsync(jobId);

                if (!applications.Any(a => a.Student.UserId == userId))
                    throw new UnauthorizedAccessException(
                        "You have no application for this job.");
            }
            else if (role == UserRoles.PlacementOfficer)
            {
                var officer = await _officerRepository.GetByUserIdAsync(userId);

                if (officer == null ||
                    (!job.IsOpenToAllColleges &&
                     !job.TargetColleges.Any(t => t.CollegeId == officer.CollegeId)))
                {
                    throw new UnauthorizedAccessException(
                        "This job is not available to your college.");
                }
            }
            else if (role != UserRoles.SuperAdmin)
            {
                throw new UnauthorizedAccessException("Access denied.");
            }

            var rounds = await _roundRepository.GetByJobIdAsync(jobId);

            return rounds.Select(r => new InterviewRoundResponseDto
            {
                InterviewRoundId = r.InterviewRoundId,
                JobId = r.JobId,
                JobTitle = job.JobTitle,
                RoundName = r.RoundName,
                RoundNumber = r.RoundNumber,
                Description = r.Description
            }).ToList();
        }
    }
}