using CampusPlacementManagement.DTOs.Interview;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IInterviewRoundService
    {
        Task<InterviewRoundResponseDto> CreateAsync(
            int jobId, CreateInterviewRoundDto dto, string userId);

        Task<List<InterviewRoundResponseDto>> GetByJobIdAsync(
            int jobId, string userId, string role);
    }
}