using CampusPlacementManagement.DTOs.Interview;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services.Implementations
{
    public class InterviewScheduleService : IInterviewScheduleService
    {
        private readonly IInterviewScheduleRepository _scheduleRepository;
        private readonly IInterviewRoundRepository _roundRepository;
        private readonly IJobApplicationRepository _applicationRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly IPlacementOfficerRepository _officerRepository;

        public InterviewScheduleService(
            IInterviewScheduleRepository scheduleRepository,
            IInterviewRoundRepository roundRepository,
            IJobApplicationRepository applicationRepository,
            ICompanyHRRepository companyHRRepository,
            IPlacementOfficerRepository officerRepository)
        {
            _scheduleRepository = scheduleRepository;
            _roundRepository = roundRepository;
            _applicationRepository = applicationRepository;
            _companyHRRepository = companyHRRepository;
            _officerRepository = officerRepository;
        }

        public async Task<InterviewScheduleResponseDto> CreateAsync(
            int applicationId, CreateInterviewScheduleDto dto, string userId)
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr == null)
                throw new UnauthorizedAccessException("Company HR profile not found.");

            var application = await _applicationRepository.GetByIdAsync(applicationId);

            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            if (application.Job.CompanyId != hr.CompanyId)
                throw new UnauthorizedAccessException(
                    "You cannot schedule an interview for this application.");

            var round = await _roundRepository.GetByIdAsync(dto.InterviewRoundId);

            if (round == null)
                throw new KeyNotFoundException("Interview round not found.");

            if (round.JobId != application.JobId)
                throw new InvalidOperationException(
                    "The interview round does not belong to this job.");

            var schedule = new InterviewSchedule
            {
                ApplicationId = applicationId,
                InterviewRoundId = dto.InterviewRoundId,
                ScheduledAt = dto.ScheduledAt,
                MeetingLink = dto.MeetingLink,
                Venue = dto.Venue
            };

            await _scheduleRepository.AddAsync(schedule);
            await _scheduleRepository.SaveChangesAsync();

            schedule.InterviewRound = round;

            return MapToDto(schedule);
        }

        public async Task<List<InterviewScheduleResponseDto>> GetByApplicationIdAsync(
            int applicationId, string userId, string role)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);

            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            if (role == "CompanyHR")
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(userId);

                if (hr == null || hr.CompanyId != application.Job.CompanyId)
                    throw new UnauthorizedAccessException("Access denied.");
            }
            else if (role == "Student")
            {
                if (application.Student.UserId != userId)
                    throw new UnauthorizedAccessException("Access denied.");
            }
            else if (role == "PlacementOfficer")
            {
                var officer = await _officerRepository.GetByUserIdAsync(userId);

                if (officer == null ||
                    officer.CollegeId != application.Student.CollegeId)
                {
                    throw new UnauthorizedAccessException("Access denied.");
                }
            }
            else if (role != "SuperAdmin")
            {
                throw new UnauthorizedAccessException("Access denied.");
            }

            var schedules = await _scheduleRepository.GetByApplicationIdAsync(
                applicationId);

            return schedules.Select(MapToDto).ToList();
        }

        public async Task<InterviewScheduleResponseDto> UpdateAsync(
            int id, UpdateInterviewScheduleDto dto, string userId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);

            if (schedule == null)
                throw new KeyNotFoundException("Interview schedule not found.");

            await CheckCompanyOwnershipAsync(schedule, userId);

            schedule.ScheduledAt = dto.ScheduledAt;
            schedule.MeetingLink = dto.MeetingLink;
            schedule.Venue = dto.Venue;
            schedule.Status = dto.Status;

            await _scheduleRepository.SaveChangesAsync();

            return MapToDto(schedule);
        }

        public async Task<InterviewScheduleResponseDto> AddFeedbackAsync(
            int id, InterviewFeedbackDto dto, string userId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);

            if (schedule == null)
                throw new KeyNotFoundException("Interview schedule not found.");

            await CheckCompanyOwnershipAsync(schedule, userId);

            if (string.IsNullOrWhiteSpace(dto.Feedback))
                throw new InvalidOperationException("Feedback is required.");

            schedule.Feedback = dto.Feedback.Trim();

            await _scheduleRepository.SaveChangesAsync();

            return MapToDto(schedule);
        }

        private async Task CheckCompanyOwnershipAsync(
            InterviewSchedule schedule, string userId)
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(userId);

            if (hr == null || hr.CompanyId != schedule.Application.Job.CompanyId)
                throw new UnauthorizedAccessException(
                    "You cannot manage this interview.");
        }

        private static InterviewScheduleResponseDto MapToDto(
            InterviewSchedule schedule)
        {
            return new InterviewScheduleResponseDto
            {
                InterviewScheduleId = schedule.InterviewScheduleId,
                ApplicationId = schedule.ApplicationId,
                InterviewRoundId = schedule.InterviewRoundId,
                RoundName = schedule.InterviewRound.RoundName,
                ScheduledAt = schedule.ScheduledAt,
                MeetingLink = schedule.MeetingLink,
                Venue = schedule.Venue,
                Status = schedule.Status,
                Feedback = schedule.Feedback
            };
        }
    }
}