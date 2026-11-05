using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class CompanyHRRepository : ICompanyHRRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyHRRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyHR?> GetByIdAsync(int id)
        {
            return await _context.CompanyHRs
                .FirstOrDefaultAsync(h => h.CompanyHRId == id);
        }

        public async Task<CompanyHR?> GetByUserIdAsync(string userId)
        {
            return await _context.CompanyHRs
                .FirstOrDefaultAsync(h => h.UserId == userId);
        }

        public async Task<List<CompanyHR>> GetAllAsync()
        {
            return await _context.CompanyHRs.ToListAsync();
        }

        public async Task AddAsync(CompanyHR companyHR)
        {
            await _context.CompanyHRs.AddAsync(companyHR);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}   