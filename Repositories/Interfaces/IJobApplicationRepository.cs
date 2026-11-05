using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(int id);
        Task<List<JobApplication>> GetByStudentIdAsync(int studentId);
        Task<List<JobApplication>> GetByJobIdAsync(int jobId);
        Task<bool> ExistsAsync(int studentId, int jobId);
        Task AddAsync(JobApplication application);
        Task SaveChangesAsync();
    }
}