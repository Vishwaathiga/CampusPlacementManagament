using CampusPlacementManagement.DTOs.Interview;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IInterviewScheduleService
    {
        Task<InterviewScheduleResponseDto> CreateAsync(
            int applicationId, CreateInterviewScheduleDto dto, string userId);

        Task<List<InterviewScheduleResponseDto>> GetByApplicationIdAsync(
            int applicationId, string userId, string role);

        Task<InterviewScheduleResponseDto> UpdateAsync(
            int id, UpdateInterviewScheduleDto dto, string userId);

        Task<InterviewScheduleResponseDto> AddFeedbackAsync(
            int id, InterviewFeedbackDto dto, string userId);
    }
}