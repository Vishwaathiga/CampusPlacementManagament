using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class PlacementOfficerRepository : IPlacementOfficerRepository
    {
        private readonly ApplicationDbContext _context;

        public PlacementOfficerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<College?> GetCollegeByIdAsync(int collegeId)
        {
            return await _context.Colleges
                .FirstOrDefaultAsync(c => c.CollegeId == collegeId);
        }

        public async Task<PlacementOfficer?> GetByIdAsync(int id)
        {
            return await _context.PlacementOfficers
                .Include(p => p.User)
                .Include(p => p.College)
                .FirstOrDefaultAsync(p => p.PlacementOfficerId == id);
        }

        public async Task<PlacementOfficer?> GetByUserIdAsync(string userId)
        {
            return await _context.PlacementOfficers
                .Include(p => p.User)
                .Include(p => p.College)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<List<PlacementOfficer>> GetAllAsync()
        {
            return await _context.PlacementOfficers
                .Include(p => p.User)
                .Include(p => p.College)
                .ToListAsync();
        }

        public async Task<List<PlacementOfficer>> GetPendingAsync()
        {
            return await _context.PlacementOfficers
                .Include(p => p.User)
                .Include(p => p.College)
                .Where(p => p.Status == PlacementOfficerStatus.Pending)
                .ToListAsync();
        }

        public async Task AddAsync(PlacementOfficer placementOfficer)
        {
            await _context.PlacementOfficers.AddAsync(placementOfficer);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}