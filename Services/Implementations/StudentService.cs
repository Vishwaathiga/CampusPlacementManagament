using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.Student;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(
            IStudentRepository studentRepository,
            IPlacementOfficerRepository officerRepository,
            UserManager<ApplicationUser> userManager)
        {
            _studentRepository = studentRepository;
            _officerRepository = officerRepository;
            _userManager = userManager;
        }

        private async Task<PlacementOfficer> GetApprovedOfficerAsync(
            string userId)
        {
            var officer = await _officerRepository.GetByUserIdAsync(userId);

            if (officer == null ||
                officer.Status != PlacementOfficerStatus.Approved ||
                officer.College.Status != CollegeStatus.Approved)
            {
                throw new InvalidOperationException(
                    "An approved placement officer from an approved college is required.");
            }

            return officer;
        }

        public async Task<StudentResponseDto> EnrollAsync(
            StudentEnrollDto dto, string officerUserId)
        {
            var officer = await GetApprovedOfficerAsync(officerUserId);

            if (string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.RegisterNumber) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Department) ||
                string.IsNullOrWhiteSpace(dto.Degree))
            {
                throw new InvalidOperationException(
                    "Please provide all required student details.");
            }

            if (dto.CGPA < 0 || dto.CGPA > 10 ||
                dto.ActiveBacklogs < 0 ||
                dto.GraduationYear < 2000)
            {
                throw new InvalidOperationException(
                    "Please provide valid academic details.");
            }

            if (await _studentRepository.RegisterNumberExistsAsync(
                officer.CollegeId, dto.RegisterNumber))
            {
                throw new InvalidOperationException(
                    "This register number already exists in your college.");
            }

            if (await _userManager.FindByEmailAsync(dto.Email) != null)
            {
                throw new InvalidOperationException(
                    "An account with this email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                AccountStatus = AccountStatus.Pending
            };

            var userResult = await _userManager.CreateAsync(user);

            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        userResult.Errors.Select(e => e.Description)));
            }

            var student = new Student
            {
                UserId = user.Id,
                CollegeId = officer.CollegeId,
                PlacementOfficerId = officer.PlacementOfficerId,
                RegisterNumber = dto.RegisterNumber,
                Department = dto.Department,
                Degree = dto.Degree,
                GraduationYear = dto.GraduationYear,
                CGPA = dto.CGPA,
                ActiveBacklogs = dto.ActiveBacklogs,
                ResumeUrl = dto.ResumeUrl,
                Status = StudentStatus.Pending
            };

            try
            {
                await _studentRepository.AddAsync(student);
                await _studentRepository.SaveChangesAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            student.User = user;

            return MapToDto(student);
        }

        public async Task<BulkStudentEnrollResultDto> BulkEnrollAsync(
            Stream file, string officerUserId)
        {
            var result = new BulkStudentEnrollResultDto();

            using var workbook = new XLWorkbook(file);

            var worksheet = workbook.Worksheet(1);

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var dto = new StudentEnrollDto
                    {
                        FullName = worksheet.Cell(row, 1).GetString(),
                        RegisterNumber = worksheet.Cell(row, 2).GetString(),
                        Email = worksheet.Cell(row, 3).GetString(),
                        PhoneNumber = worksheet.Cell(row, 4).GetString(),
                        Department = worksheet.Cell(row, 5).GetString(),
                        Degree = worksheet.Cell(row, 6).GetString(),
                        GraduationYear = worksheet.Cell(row, 7).GetValue<int>(),
                        CGPA = worksheet.Cell(row, 8).GetValue<decimal>(),
                        ActiveBacklogs = worksheet.Cell(row, 9).GetValue<int>(),
                        ResumeUrl = worksheet.Cell(row, 10).GetString()
                    };

                    await EnrollAsync(dto, officerUserId);
                    result.SuccessfulEnrollments++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {row}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<List<StudentResponseDto>> GetAllAsync(
            string userId, bool isSuperAdmin)
        {
            int? collegeId = null;

            if (!isSuperAdmin)
            {
                var officer = await GetApprovedOfficerAsync(userId);
                collegeId = officer.CollegeId;
            }

            var students = await _studentRepository.GetAllAsync(collegeId);

            return students.Select(MapToDto).ToList();
        }

        public async Task<List<StudentResponseDto>> GetPendingAsync(
            string officerUserId)
        {
            var officer = await GetApprovedOfficerAsync(officerUserId);

            var students = await _studentRepository.GetPendingAsync(
                officer.CollegeId);

            return students.Select(MapToDto).ToList();
        }

        public async Task<StudentResponseDto?> GetByIdAsync(
            int id, string userId, bool isSuperAdmin,
            bool isPlacementOfficer)
        {
            var student = await _studentRepository.GetByIdAsync(id);

            if (student == null)
            {
                return null;
            }

            if (isSuperAdmin)
            {
                return MapToDto(student);
            }

            if (isPlacementOfficer)
            {
                var officer = await GetApprovedOfficerAsync(userId);

                if (student.CollegeId != officer.CollegeId)
                {
                    throw new UnauthorizedAccessException(
                        "You cannot view students from another college.");
                }

                return MapToDto(student);
            }

            if (student.UserId == userId)
            {
                return MapToDto(student);
            }

            throw new UnauthorizedAccessException(
                "You are not allowed to view this student.");
        }

        public async Task<StudentResponseDto> ApproveAsync(
            int id, string officerUserId)
        {
            var officer = await GetApprovedOfficerAsync(officerUserId);

            var student = await _studentRepository.GetByIdAsync(id);

            if (student == null)
            {
                throw new KeyNotFoundException("Student not found.");
            }

            if (student.CollegeId != officer.CollegeId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot approve students from another college.");
            }

            if (student.Status != StudentStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending students can be approved.");
            }

            student.Status = StudentStatus.Approved;
            student.ApprovedAt = DateTime.UtcNow;
            student.User.AccountStatus = AccountStatus.Approved;

            var updateResult = await _userManager.UpdateAsync(student.User);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        updateResult.Errors.Select(e => e.Description)));
            }

            var roleResult = await _userManager.AddToRoleAsync(
                student.User, UserRoles.Student);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        roleResult.Errors.Select(e => e.Description)));
            }

            await _studentRepository.SaveChangesAsync();

            return MapToDto(student);
        }

        public async Task<StudentResponseDto> RejectAsync(
            int id, string officerUserId)
        {
            var officer = await GetApprovedOfficerAsync(officerUserId);

            var student = await _studentRepository.GetByIdAsync(id);

            if (student == null)
            {
                throw new KeyNotFoundException("Student not found.");
            }

            if (student.CollegeId != officer.CollegeId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot reject students from another college.");
            }

            if (student.Status != StudentStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending students can be rejected.");
            }

            student.Status = StudentStatus.Rejected;
            student.User.AccountStatus = AccountStatus.Rejected;

            var updateResult = await _userManager.UpdateAsync(student.User);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        updateResult.Errors.Select(e => e.Description)));
            }

            await _studentRepository.SaveChangesAsync();

            return MapToDto(student);
        }

        public async Task<StudentResponseDto> SuspendAsync(
            int id, string userId, bool isSuperAdmin)
        {
            var student = await _studentRepository.GetByIdAsync(id);

            if (student == null)
            {
                throw new KeyNotFoundException("Student not found.");
            }

            if (!isSuperAdmin)
            {
                var officer = await GetApprovedOfficerAsync(userId);

                if (student.CollegeId != officer.CollegeId)
                {
                    throw new UnauthorizedAccessException(
                        "You cannot suspend students from another college.");
                }
            }

            if (student.Status != StudentStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Only approved students can be suspended.");
            }

            student.Status = StudentStatus.Suspended;
            student.User.AccountStatus = AccountStatus.Suspended;

            var updateResult = await _userManager.UpdateAsync(student.User);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        updateResult.Errors.Select(e => e.Description)));
            }

            await _studentRepository.SaveChangesAsync();

            return MapToDto(student);
        }

        public async Task<StudentResponseDto> UpdateAsync(
            int id, StudentUpdateDto dto, string userId,
            bool isSuperAdmin, bool isPlacementOfficer, bool isStudent)
        {
            var student = await _studentRepository.GetByIdAsync(id);

            if (student == null)
            {
                throw new KeyNotFoundException("Student not found.");
            }

            if (isStudent)
            {
                if (student.UserId != userId)
                {
                    throw new UnauthorizedAccessException(
                        "You can only update your own profile.");
                }

                if (dto.Department != null || dto.Degree != null ||
                    dto.GraduationYear.HasValue || dto.CGPA.HasValue ||
                    dto.ActiveBacklogs.HasValue)
                {
                    throw new UnauthorizedAccessException(
                        "Students cannot update their academic details.");
                }
            }
            else if (isPlacementOfficer)
            {
                var officer = await GetApprovedOfficerAsync(userId);

                if (student.CollegeId != officer.CollegeId)
                {
                    throw new UnauthorizedAccessException(
                        "You cannot update students from another college.");
                }
            }
            else if (!isSuperAdmin)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to update this student.");
            }

            if (dto.CGPA.HasValue &&
                (dto.CGPA < 0 || dto.CGPA > 10))
            {
                throw new InvalidOperationException("Invalid CGPA.");
            }

            if (dto.ActiveBacklogs.HasValue && dto.ActiveBacklogs < 0)
            {
                throw new InvalidOperationException("Invalid backlog count.");
            }

            if (dto.GraduationYear.HasValue &&
                dto.GraduationYear < 2000)
            {
                throw new InvalidOperationException("Invalid graduation year.");
            }

            if (dto.PhoneNumber != null)
                student.User.PhoneNumber = dto.PhoneNumber;

            if (dto.Department != null)
                student.Department = dto.Department;

            if (dto.Degree != null)
                student.Degree = dto.Degree;

            if (dto.GraduationYear.HasValue)
                student.GraduationYear = dto.GraduationYear.Value;

            if (dto.CGPA.HasValue)
                student.CGPA = dto.CGPA.Value;

            if (dto.ActiveBacklogs.HasValue)
                student.ActiveBacklogs = dto.ActiveBacklogs.Value;

            if (dto.ResumeUrl != null)
                student.ResumeUrl = dto.ResumeUrl;

            var updateResult = await _userManager.UpdateAsync(student.User);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        updateResult.Errors.Select(e => e.Description)));
            }

            await _studentRepository.SaveChangesAsync();

            return MapToDto(student);
        }

        public async Task<StudentResponseDto?> GetMyProfileAsync(string userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);

            return student == null ? null : MapToDto(student);
        }

        private static StudentResponseDto MapToDto(Student student)
        {
            return new StudentResponseDto
            {
                StudentId = student.StudentId,
                UserId = student.UserId,
                FullName = student.User.FullName,
                Email = student.User.Email ?? string.Empty,
                PhoneNumber = student.User.PhoneNumber ?? string.Empty,
                CollegeId = student.CollegeId,
                PlacementOfficerId = student.PlacementOfficerId,
                RegisterNumber = student.RegisterNumber,
                Department = student.Department,
                Degree = student.Degree,
                GraduationYear = student.GraduationYear,
                CGPA = student.CGPA,
                ActiveBacklogs = student.ActiveBacklogs,
                ResumeUrl = student.ResumeUrl,
                Status = student.Status,
                ApprovedAt = student.ApprovedAt
            };
        }
    }
}