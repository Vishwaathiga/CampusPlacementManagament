using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IInterviewRoundRepository
    {
        Task<InterviewRound?> GetByIdAsync(int id);

        Task<List<InterviewRound>> GetByJobIdAsync(int jobId);

        Task<bool> ExistsRoundNumberAsync(int jobId, int roundNumber);

        Task AddAsync(InterviewRound round);

        Task SaveChangesAsync();
    }
}