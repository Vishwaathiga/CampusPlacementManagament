using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IInterviewScheduleRepository
    {
        Task<InterviewSchedule?> GetByIdAsync(int id);

        Task<List<InterviewSchedule>> GetByApplicationIdAsync(int applicationId);

        Task AddAsync(InterviewSchedule schedule);

        Task SaveChangesAsync();
    }
}