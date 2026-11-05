using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface ICompanyHRRepository
    {
        Task<CompanyHR?> GetByIdAsync(int id);
        Task<CompanyHR?> GetByUserIdAsync(string userId);
        Task<List<CompanyHR>> GetAllAsync();
        Task AddAsync(CompanyHR companyHR);
        Task SaveChangesAsync();
    }
}