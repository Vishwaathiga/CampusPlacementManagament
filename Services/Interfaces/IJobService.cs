using CampusPlacementManagement.DTOs.Job;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateAsync(CreateJobDto dto, string userId);
        Task<List<JobResponseDto>> GetAllAsync(string userId, string role);
        Task<JobResponseDto?> GetByIdAsync(int id, string userId, string role);
        Task<JobResponseDto> UpdateAsync(int id, UpdateJobDto dto, string userId);
        Task<JobResponseDto> PublishAsync(int id, string userId);
        Task<JobResponseDto> CloseAsync(int id, string userId);
        Task DeleteAsync(int id, string userId);
        Task<List<JobResponseDto>> GetEligibleJobsAsync(string userId);
        Task<bool> CheckEligibilityAsync(int jobId, string userId);
        Task<List<int>> GetTargetCollegesAsync(int jobId, string userId);
        Task AddTargetCollegesAsync(int jobId, TargetCollegesDto dto, string userId);
        Task RemoveTargetCollegeAsync(int jobId, int collegeId, string userId);
    }
}