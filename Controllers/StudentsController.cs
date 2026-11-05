using System.Security.Claims;
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.Student;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/students")]
    [ApiController]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost("enroll")]
        [Authorize(Roles = UserRoles.PlacementOfficer)]
        public async Task<IActionResult> Enroll(StudentEnrollDto dto)
        {
            try
            {
                var result = await _studentService.EnrollAsync(
                    dto, CurrentUserId);

                return StatusCode(201, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bulk-enroll")]
        [Authorize(Roles = UserRoles.PlacementOfficer)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BulkEnroll(IFormFile file)
        {
            if (file == null || file.Length == 0 ||
                !Path.GetExtension(file.FileName).Equals(
                    ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Please upload a valid Excel .xlsx file."
                });
            }

            using var stream = file.OpenReadStream();

            try
            {
                var result = await _studentService.BulkEnrollAsync(
                    stream, CurrentUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.PlacementOfficer)]
        public async Task<IActionResult> GetAll()
        {
            var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);

            return Ok(await _studentService.GetAllAsync(
                CurrentUserId, isSuperAdmin));
        }

        [HttpGet("pending")]
        [Authorize(Roles = UserRoles.PlacementOfficer)]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                return Ok(await _studentService.GetPendingAsync(CurrentUserId));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," +
                           UserRoles.PlacementOfficer + "," +
                           UserRoles.Student)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _studentService.GetByIdAsync(
                    id,
                    CurrentUserId,
                    User.IsInRole(UserRoles.SuperAdmin),
                    User.IsInRole(UserRoles.PlacementOfficer));

                if (result == null)
                    return NotFound(new { message = "Student not found." });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = UserRoles.PlacementOfficer)]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                return Ok(await _studentService.ApproveAsync(id, CurrentUserId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = UserRoles.PlacementOfficer)]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                return Ok(await _studentService.RejectAsync(id, CurrentUserId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/suspend")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," +
                           UserRoles.PlacementOfficer)]
        public async Task<IActionResult> Suspend(int id)
        {
            try
            {
                return Ok(await _studentService.SuspendAsync(
                    id, CurrentUserId, User.IsInRole(UserRoles.SuperAdmin)));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," +
                           UserRoles.PlacementOfficer + "," +
                           UserRoles.Student)]
        public async Task<IActionResult> Update(
            int id, StudentUpdateDto dto)
        {
            try
            {
                var result = await _studentService.UpdateAsync(
                    id,
                    dto,
                    CurrentUserId,
                    User.IsInRole(UserRoles.SuperAdmin),
                    User.IsInRole(UserRoles.PlacementOfficer),
                    User.IsInRole(UserRoles.Student));

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _studentService.GetMyProfileAsync(CurrentUserId);

            if (result == null)
                return NotFound(new { message = "Student profile not found." });

            return Ok(result);
        }
    }
}