using CampusPlacementManagement.DTOs.JobApplication;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IJobApplicationService
    {
        Task<JobApplicationResponseDto> ApplyAsync(ApplyJobDto dto, string userId);
        Task<List<JobApplicationResponseDto>> GetMyApplicationsAsync(string userId);
        Task<List<JobApplicationResponseDto>> GetApplicationsByJobAsync(int jobId, string userId);
        Task<JobApplicationResponseDto?> GetByIdAsync(int id, string userId);
        Task<JobApplicationResponseDto> UpdateStatusAsync(
            int id, UpdateApplicationStatusDto dto, string userId);
        Task<List<ApplicationHistoryResponseDto>> GetHistoryAsync(int id, string userId);
        Task WithdrawAsync(int id, string userId);
    }
}