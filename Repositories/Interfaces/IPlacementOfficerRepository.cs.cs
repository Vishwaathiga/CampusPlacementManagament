using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IPlacementOfficerRepository
    {
        Task<College?> GetCollegeByIdAsync(int collegeId);

        Task<PlacementOfficer?> GetByIdAsync(int id);

        Task<PlacementOfficer?> GetByUserIdAsync(string userId);

        Task<List<PlacementOfficer>> GetAllAsync();

        Task<List<PlacementOfficer>> GetPendingAsync();

        Task AddAsync(PlacementOfficer placementOfficer);

        Task SaveChangesAsync();
    }
}