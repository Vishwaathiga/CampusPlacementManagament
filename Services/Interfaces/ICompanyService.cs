using CampusPlacementManagement.DTOs.Company;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyResponseDto> RegisterAsync(CompanyRegisterDto dto);
        Task<List<CompanyResponseDto>> GetAllAsync();
        Task<List<CompanyResponseDto>> GetPendingAsync();
        Task<CompanyResponseDto?> GetByIdAsync(int id);
        Task<CompanyResponseDto> UpdateAsync(int id, CompanyUpdateDto dto);
        Task<CompanyResponseDto> ApproveAsync(int id);
        Task<CompanyResponseDto> RejectAsync(int id);
        Task<CompanyResponseDto> SuspendAsync(int id);
    }
}