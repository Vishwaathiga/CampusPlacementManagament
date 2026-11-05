using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.CompanyHR;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services.Implementations
{
    public class CompanyHRService : ICompanyHRService
    {
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyHRService(
            ICompanyHRRepository companyHRRepository,
            ICompanyRepository companyRepository,
            UserManager<ApplicationUser> userManager)
        {
            _companyHRRepository = companyHRRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
        }

        public async Task<CompanyHRResponseDto> RegisterAsync(
            CompanyHRRegisterDto dto)
        {
            var company = await _companyRepository.GetByIdAsync(dto.CompanyId);

            if (company == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }

            if (company.Status != CompanyStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Only approved companies can register HR representatives.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                AccountStatus = AccountStatus.Pending
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var companyHR = new CompanyHR
            {
                UserId = user.Id,
                CompanyId = dto.CompanyId,
                Designation = dto.Designation
            };

            try
            {
                await _companyHRRepository.AddAsync(companyHR);
                await _companyHRRepository.SaveChangesAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            return MapToDto(companyHR, user);
        }

        public async Task<List<CompanyHRResponseDto>> GetAllAsync()
        {
            var companyHRs = await _companyHRRepository.GetAllAsync();
            var response = new List<CompanyHRResponseDto>();

            foreach (var companyHR in companyHRs)
            {
                var user = await _userManager.FindByIdAsync(companyHR.UserId);

                if (user != null)
                {
                    response.Add(MapToDto(companyHR, user));
                }
            }

            return response;
        }

        public async Task<List<CompanyHRResponseDto>> GetPendingAsync()
        {
            var companyHRs = await GetAllAsync();

            return companyHRs
                .Where(h => h.AccountStatus == AccountStatus.Pending)
                .ToList();
        }

        public async Task<CompanyHRResponseDto> ApproveAsync(
            int id, string approvedBy)
        {
            var companyHR = await _companyHRRepository.GetByIdAsync(id);

            if (companyHR == null)
            {
                throw new KeyNotFoundException("Company HR not found.");
            }

            var user = await _userManager.FindByIdAsync(companyHR.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException("User account not found.");
            }

            if (user.AccountStatus != AccountStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending HR accounts can be approved.");
            }

            user.AccountStatus = AccountStatus.Approved;
            companyHR.ApprovedBy = approvedBy;
            companyHR.ApprovedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user, UserRoles.CompanyHR);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            await _companyHRRepository.SaveChangesAsync();

            return MapToDto(companyHR, user);
        }

        public async Task<CompanyHRResponseDto> RejectAsync(int id)
        {
            var companyHR = await _companyHRRepository.GetByIdAsync(id);

            if (companyHR == null)
            {
                throw new KeyNotFoundException("Company HR not found.");
            }

            var user = await _userManager.FindByIdAsync(companyHR.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException("User account not found.");
            }

            if (user.AccountStatus != AccountStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending HR accounts can be rejected.");
            }

            user.AccountStatus = AccountStatus.Rejected;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return MapToDto(companyHR, user);
        }

        public async Task<CompanyHRResponseDto?> GetMyProfileAsync(string userId)
        {
            var companyHR = await _companyHRRepository.GetByUserIdAsync(userId);

            if (companyHR == null)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            return MapToDto(companyHR, user);
        }

        private static CompanyHRResponseDto MapToDto(
            CompanyHR companyHR, ApplicationUser user)
        {
            return new CompanyHRResponseDto
            {
                CompanyHRId = companyHR.CompanyHRId,
                UserId = companyHR.UserId,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                CompanyId = companyHR.CompanyId,
                Designation = companyHR.Designation,
                AccountStatus = user.AccountStatus,
                ApprovedAt = companyHR.ApprovedAt
            };
        }
    }
}