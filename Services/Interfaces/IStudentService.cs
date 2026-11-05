using CampusPlacementManagement.DTOs.Student;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IStudentService
    {
        Task<StudentResponseDto> EnrollAsync(
            StudentEnrollDto dto, string officerUserId);

        Task<BulkStudentEnrollResultDto> BulkEnrollAsync(
            Stream file, string officerUserId);

        Task<List<StudentResponseDto>> GetAllAsync(
            string userId, bool isSuperAdmin);

        Task<List<StudentResponseDto>> GetPendingAsync(string officerUserId);

        Task<StudentResponseDto?> GetByIdAsync(
            int id, string userId, bool isSuperAdmin,
            bool isPlacementOfficer);

        Task<StudentResponseDto> ApproveAsync(int id, string officerUserId);

        Task<StudentResponseDto> RejectAsync(int id, string officerUserId);

        Task<StudentResponseDto> SuspendAsync(
            int id, string userId, bool isSuperAdmin);

        Task<StudentResponseDto> UpdateAsync(
            int id, StudentUpdateDto dto, string userId,
            bool isSuperAdmin, bool isPlacementOfficer, bool isStudent);

        Task<StudentResponseDto?> GetMyProfileAsync(string userId);
    }
}