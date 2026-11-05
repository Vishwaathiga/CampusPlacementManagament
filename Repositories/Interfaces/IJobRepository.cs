using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IJobRepository
    {
        Task<JobOpening?> GetByIdAsync(int id);
        Task<List<JobOpening>> GetAllAsync();
        Task AddAsync(JobOpening job);
        void Delete(JobOpening job);
        Task SaveChangesAsync();
    }
}