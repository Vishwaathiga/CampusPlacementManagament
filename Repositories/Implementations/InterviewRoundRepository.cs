using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class InterviewRoundRepository : IInterviewRoundRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewRoundRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InterviewRound?> GetByIdAsync(int id)
        {
            return await _context.InterviewRounds
                .Include(r => r.Job)
                .FirstOrDefaultAsync(r => r.InterviewRoundId == id);
        }

        public async Task<List<InterviewRound>> GetByJobIdAsync(int jobId)
        {
            return await _context.InterviewRounds
                .Where(r => r.JobId == jobId)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync();
        }

        public async Task<bool> ExistsRoundNumberAsync(
            int jobId, int roundNumber)
        {
            return await _context.InterviewRounds.AnyAsync(
                r => r.JobId == jobId && r.RoundNumber == roundNumber);
        }

        public async Task AddAsync(InterviewRound round)
        {
            await _context.InterviewRounds.AddAsync(round);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}