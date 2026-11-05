using CampusPlacementManagement.DTOs.Company;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services.Implementations
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<CompanyResponseDto> RegisterAsync(CompanyRegisterDto dto)
        {
            var company = new Company
            {
                CompanyName = dto.CompanyName,
                CompanyEmail = dto.CompanyEmail,
                Website = dto.Website,
                Industry = dto.Industry,
                Address = dto.Address,
                Description = dto.Description,
                Status = CompanyStatus.Pending
            };

            await _companyRepository.AddAsync(company);
            await _companyRepository.SaveChangesAsync();

            return MapToDto(company);
        }

        public async Task<List<CompanyResponseDto>> GetAllAsync()
        {
            var companies = await _companyRepository.GetAllAsync();

            return companies.Select(MapToDto).ToList();
        }

        public async Task<List<CompanyResponseDto>> GetPendingAsync()
        {
            var companies = await _companyRepository.GetPendingAsync();

            return companies.Select(MapToDto).ToList();
        }

        public async Task<CompanyResponseDto?> GetByIdAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);

            return company == null ? null : MapToDto(company);
        }

        public async Task<CompanyResponseDto> UpdateAsync(int id, CompanyUpdateDto dto)
        {
            var company = await _companyRepository.GetByIdAsync(id);

            if (company == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }

            company.CompanyName = dto.CompanyName;
            company.CompanyEmail = dto.CompanyEmail;
            company.Website = dto.Website;
            company.Industry = dto.Industry;
            company.Address = dto.Address;
            company.Description = dto.Description;

            await _companyRepository.SaveChangesAsync();

            return MapToDto(company);
        }

        public async Task<CompanyResponseDto> ApproveAsync(int id)
        {
            var company = await GetCompanyOrThrowAsync(id);

            if (company.Status != CompanyStatus.Pending)
            {
                throw new InvalidOperationException("Only pending companies can be approved.");
            }

            company.Status = CompanyStatus.Approved;

            await _companyRepository.SaveChangesAsync();

            return MapToDto(company);
        }

        public async Task<CompanyResponseDto> RejectAsync(int id)
        {
            var company = await GetCompanyOrThrowAsync(id);

            if (company.Status != CompanyStatus.Pending)
            {
                throw new InvalidOperationException("Only pending companies can be rejected.");
            }

            company.Status = CompanyStatus.Rejected;

            await _companyRepository.SaveChangesAsync();

            return MapToDto(company);
        }

        public async Task<CompanyResponseDto> SuspendAsync(int id)
        {
            var company = await GetCompanyOrThrowAsync(id);

            if (company.Status != CompanyStatus.Approved)
            {
                throw new InvalidOperationException("Only approved companies can be suspended.");
            }

            company.Status = CompanyStatus.Suspended;

            await _companyRepository.SaveChangesAsync();

            return MapToDto(company);
        }

        private async Task<Company> GetCompanyOrThrowAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);

            if (company == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }

            return company;
        }

        private static CompanyResponseDto MapToDto(Company company)
        {
            return new CompanyResponseDto
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                CompanyEmail = company.CompanyEmail,
                Website = company.Website,
                Industry = company.Industry,
                Address = company.Address,
                Description = company.Description,
                Status = company.Status,
                CreatedAt = company.CreatedAt
            };
        }
    }
}