using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company?> GetByIdAsync(int id);
        Task<List<Company>> GetAllAsync();
        Task<List<Company>> GetPendingAsync();
        Task AddAsync(Company company);
        Task SaveChangesAsync();
    }
}