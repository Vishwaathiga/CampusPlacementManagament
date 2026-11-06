using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class InterviewScheduleRepository : IInterviewScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewScheduleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InterviewSchedule?> GetByIdAsync(int id)
        {
            return await _context.InterviewSchedules
                .Include(i => i.InterviewRound)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Job)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(i => i.InterviewScheduleId == id);
        }

        public async Task<List<InterviewSchedule>> GetByApplicationIdAsync(
            int applicationId)
        {
            return await _context.InterviewSchedules
                .Include(i => i.InterviewRound)
                .Where(i => i.ApplicationId == applicationId)
                .OrderBy(i => i.ScheduledAt)
                .ToListAsync();
        }

        public async Task AddAsync(InterviewSchedule schedule)
        {
            await _context.InterviewSchedules.AddAsync(schedule);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}