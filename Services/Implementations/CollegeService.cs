using CampusPlacementManagement.DTOs.College;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services.Implementations
{
    public class CollegeService : ICollegeService
    {
        private readonly ICollegeRepository _collegeRepository;

        public CollegeService(ICollegeRepository collegeRepository)
        {
            _collegeRepository = collegeRepository;
        }

        public async Task<IEnumerable<CollegeResponseDto>> GetAllAsync()
        {
            var colleges = await _collegeRepository.GetAllAsync();
            return colleges.Select(MapToResponse);
        }

        public async Task<CollegeResponseDto?> GetByIdAsync(int id)
        {
            var college = await _collegeRepository.GetByIdAsync(id);
            return college == null ? null : MapToResponse(college);
        }

        public async Task<CollegeResponseDto> RegisterAsync(CollegeRegisterDto request)
        {
            var existingCollege = await _collegeRepository.GetByCodeAsync(request.CollegeCode);

            if (existingCollege != null)
                throw new InvalidOperationException("College code already exists.");

            var college = new College
            {
                CollegeName = request.CollegeName,
                CollegeCode = request.CollegeCode,
                UniversityName = request.UniversityName,
                OfficialEmail = request.OfficialEmail,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Website = request.Website,
                Status = CollegeStatus.Pending
            };

            await _collegeRepository.AddAsync(college);

            return MapToResponse(college);
        }

        public async Task<CollegeResponseDto?> UpdateAsync(int id, CollegeUpdateDto request)
        {
            var college = await _collegeRepository.GetByIdAsync(id);

            if (college == null)
                return null;

            var existingCollege = await _collegeRepository.GetByCodeAsync(request.CollegeCode);

            if (existingCollege != null && existingCollege.CollegeId != id)
                throw new InvalidOperationException("College code already exists.");

            college.CollegeName = request.CollegeName;
            college.CollegeCode = request.CollegeCode;
            college.UniversityName = request.UniversityName;
            college.OfficialEmail = request.OfficialEmail;
            college.PhoneNumber = request.PhoneNumber;
            college.Address = request.Address;
            college.Website = request.Website;

            await _collegeRepository.UpdateAsync(college);

            return MapToResponse(college);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var college = await _collegeRepository.GetByIdAsync(id);

            if (college == null)
                return false;

            await _collegeRepository.DeleteAsync(college);
            return true;
        }

        public async Task<CollegeResponseDto?> UpdateStatusAsync(int id, CollegeApprovalDto request)
        {
            var college = await _collegeRepository.GetByIdAsync(id);

            if (college == null)
                return null;

            college.Status = request.Status;

            await _collegeRepository.UpdateAsync(college);

            return MapToResponse(college);
        }

        private static CollegeResponseDto MapToResponse(College college)
        {
            return new CollegeResponseDto
            {
                CollegeId = college.CollegeId,
                CollegeName = college.CollegeName,
                CollegeCode = college.CollegeCode,
                UniversityName = college.UniversityName,
                OfficialEmail = college.OfficialEmail,
                PhoneNumber = college.PhoneNumber,
                Address = college.Address,
                Website = college.Website,
                Status = college.Status,
                CreatedAt = college.CreatedAt
            };
        }
    }
}