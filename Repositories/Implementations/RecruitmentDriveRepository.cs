
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class RecruitmentDriveRepository : IRecruitmentDriveRepository
    {
        private readonly ApplicationDbContext _context;

        public RecruitmentDriveRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecruitmentDrive?> GetByIdAsync(int id)
        {
            return await _context.RecruitmentDrives
                .Include(d => d.Job)
                    .ThenInclude(j => j.Company)
                .Include(d => d.College)
                .FirstOrDefaultAsync(d => d.DriveId == id);
        }

        public async Task<List<RecruitmentDrive>> GetAllAsync()
        {
            return await _context.RecruitmentDrives
                .Include(d => d.Job)
                    .ThenInclude(j => j.Company)
                .Include(d => d.College)
                .ToListAsync();
        }

        public async Task AddAsync(RecruitmentDrive drive)
        {
            await _context.RecruitmentDrives.AddAsync(drive);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
