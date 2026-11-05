using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobOpening?> GetByIdAsync(int id)
        {
            return await _context.JobOpenings
                .Include(j => j.TargetColleges)
                .Include(j => j.EligibleDepartments)
                .Include(j => j.EligibleGraduationYears)
                .FirstOrDefaultAsync(j => j.JobId == id);
        }

        public async Task<List<JobOpening>> GetAllAsync()
        {
            return await _context.JobOpenings
                .Include(j => j.TargetColleges)
                .Include(j => j.EligibleDepartments)
                .Include(j => j.EligibleGraduationYears)
                .ToListAsync();
        }

        public async Task AddAsync(JobOpening job)
        {
            await _context.JobOpenings.AddAsync(job);
        }

        public void Delete(JobOpening job)
        {
            _context.JobOpenings.Remove(job);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}