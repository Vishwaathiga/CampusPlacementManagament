using CampusPlacementManagement.DTOs.College;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface ICollegeService
    {
        Task<IEnumerable<CollegeResponseDto>> GetAllAsync();
        Task<CollegeResponseDto?> GetByIdAsync(int id);
        Task<CollegeResponseDto> RegisterAsync(CollegeRegisterDto request);
        Task<CollegeResponseDto?> UpdateAsync(int id, CollegeUpdateDto request);
        Task<bool> DeleteAsync(int id);
        Task<CollegeResponseDto?> UpdateStatusAsync(int id, CollegeApprovalDto request);
    }
}