
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IRecruitmentDriveRepository
    {
        Task<RecruitmentDrive?> GetByIdAsync(int id);
        Task<List<RecruitmentDrive>> GetAllAsync();
        Task AddAsync(RecruitmentDrive drive);
        Task SaveChangesAsync();
    }
}
