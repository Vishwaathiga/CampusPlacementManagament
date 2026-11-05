using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication?> GetByIdAsync(int id)
        {
            return await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.StatusHistory)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);
        }

        public async Task<List<JobApplication>> GetByStudentIdAsync(int studentId)
        {
            return await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<List<JobApplication>> GetByJobIdAsync(int jobId)
        {
            return await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Where(a => a.JobId == jobId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int studentId, int jobId)
        {
            return await _context.JobApplications
                .AnyAsync(a => a.StudentId == studentId && a.JobId == jobId);
        }

        public async Task AddAsync(JobApplication application)
        {
            await _context.JobApplications.AddAsync(application);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}