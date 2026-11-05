using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.PlacementOfficer;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services.Implementations
{
    public class PlacementOfficerService : IPlacementOfficerService
    {
        private readonly IPlacementOfficerRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PlacementOfficerService(
            IPlacementOfficerRepository repository,
            UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<PlacementOfficerResponseDto> RegisterAsync(
            PlacementOfficerRegisterDto dto)
        {
            var college = await _repository.GetCollegeByIdAsync(dto.CollegeId);

            if (college == null || college.Status != CollegeStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Selected college is not approved.");
            }

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "Email is already registered.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                AccountStatus = AccountStatus.Pending
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var placementOfficer = new PlacementOfficer
            {
                UserId = user.Id,
                CollegeId = dto.CollegeId,
                Designation = dto.Designation,
                Status = PlacementOfficerStatus.Pending
            };

            await _repository.AddAsync(placementOfficer);
            await _repository.SaveChangesAsync();

            var savedOfficer = await _repository.GetByUserIdAsync(user.Id);

            return MapToDto(savedOfficer!);
        }

        public async Task<List<PlacementOfficerResponseDto>> GetPendingAsync()
        {
            var officers = await _repository.GetPendingAsync();

            return officers.Select(MapToDto).ToList();
        }

        public async Task<List<PlacementOfficerResponseDto>> GetAllAsync()
        {
            var officers = await _repository.GetAllAsync();

            return officers.Select(MapToDto).ToList();
        }

        public async Task<PlacementOfficerResponseDto?> GetByIdAsync(int id)
        {
            var officer = await _repository.GetByIdAsync(id);

            return officer == null ? null : MapToDto(officer);
        }

        public async Task<PlacementOfficerResponseDto?> GetMyProfileAsync(
            string userId)
        {
            var officer = await _repository.GetByUserIdAsync(userId);

            return officer == null ? null : MapToDto(officer);
        }

        public async Task<PlacementOfficerResponseDto> ApproveAsync(
            int id, string approverId)
        {
            var officer = await _repository.GetByIdAsync(id);

            if (officer == null)
            {
                throw new KeyNotFoundException("Placement officer not found.");
            }

            if (officer.Status != PlacementOfficerStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending placement officers can be approved.");
            }

            if (officer.College.Status != CollegeStatus.Approved)
            {
                throw new InvalidOperationException(
                    "The officer's college is not approved.");
            }

            officer.Status = PlacementOfficerStatus.Approved;
            officer.ApprovedBy = approverId;
            officer.ApprovedAt = DateTime.UtcNow;

            officer.User.AccountStatus = AccountStatus.Approved;

            var result = await _userManager.UpdateAsync(officer.User);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await _userManager.IsInRoleAsync(
                officer.User, UserRoles.PlacementOfficer))
            {
                var roleResult = await _userManager.AddToRoleAsync(
                    officer.User, UserRoles.PlacementOfficer);

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            await _repository.SaveChangesAsync();

            return MapToDto(officer);
        }

        public async Task<PlacementOfficerResponseDto> RejectAsync(int id)
        {
            var officer = await _repository.GetByIdAsync(id);

            if (officer == null)
            {
                throw new KeyNotFoundException("Placement officer not found.");
            }

            officer.Status = PlacementOfficerStatus.Rejected;
            officer.User.AccountStatus = AccountStatus.Rejected;

            var result = await _userManager.UpdateAsync(officer.User);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _repository.SaveChangesAsync();

            return MapToDto(officer);
        }

        public async Task<PlacementOfficerResponseDto> SuspendAsync(int id)
        {
            var officer = await _repository.GetByIdAsync(id);

            if (officer == null)
            {
                throw new KeyNotFoundException("Placement officer not found.");
            }

            officer.Status = PlacementOfficerStatus.Suspended;
            officer.User.AccountStatus = AccountStatus.Suspended;

            var result = await _userManager.UpdateAsync(officer.User);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _repository.SaveChangesAsync();

            return MapToDto(officer);
        }

        private static PlacementOfficerResponseDto MapToDto(
            PlacementOfficer officer)
        {
            return new PlacementOfficerResponseDto
            {
                PlacementOfficerId = officer.PlacementOfficerId,
                FullName = officer.User.FullName,
                Email = officer.User.Email ?? string.Empty,
                PhoneNumber = officer.User.PhoneNumber ?? string.Empty,
                CollegeId = officer.CollegeId,
                CollegeName = officer.College.CollegeName,
                Designation = officer.Designation,
                Status = officer.Status,
                ApprovedAt = officer.ApprovedAt
            };
        }
    }
}