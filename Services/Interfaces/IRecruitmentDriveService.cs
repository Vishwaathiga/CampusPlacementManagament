
using CampusPlacementManagement.DTOs.RecruitmentDrive;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IRecruitmentDriveService
    {
        Task<RecruitmentDriveResponseDto> CreateAsync(
            CreateRecruitmentDriveDto dto, string userId);

        Task<List<RecruitmentDriveResponseDto>> GetAllAsync(
            string userId, string role);

        Task<RecruitmentDriveResponseDto?> GetByIdAsync(
            int id, string userId, string role);

        Task<RecruitmentDriveResponseDto> UpdateAsync(
    int id, UpdateRecruitmentDriveDto dto, string userId, string role);

        Task CancelAsync(int id, string userId, string role);
    }
}
