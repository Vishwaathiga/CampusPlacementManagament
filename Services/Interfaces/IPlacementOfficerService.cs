using CampusPlacementManagement.DTOs.PlacementOfficer;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IPlacementOfficerService
    {
        Task<PlacementOfficerResponseDto> RegisterAsync(
            PlacementOfficerRegisterDto dto);

        Task<List<PlacementOfficerResponseDto>> GetPendingAsync();

        Task<List<PlacementOfficerResponseDto>> GetAllAsync();

        Task<PlacementOfficerResponseDto?> GetByIdAsync(int id);

        Task<PlacementOfficerResponseDto?> GetMyProfileAsync(string userId);

        Task<PlacementOfficerResponseDto> ApproveAsync(
            int id, string approverId);

        Task<PlacementOfficerResponseDto> RejectAsync(int id);

        Task<PlacementOfficerResponseDto> SuspendAsync(int id);
    }
}