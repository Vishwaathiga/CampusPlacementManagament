
using System.Security.Claims;
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.RecruitmentDrive;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/recruitment-drives")]
    [ApiController]
    [Authorize]
    public class RecruitmentDrivesController : ControllerBase
    {
        private readonly IRecruitmentDriveService _service;

        public RecruitmentDrivesController(
            IRecruitmentDriveService service)
        {
            _service = service;
        }

        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private string UserRole =>
            User.IsInRole(UserRoles.SuperAdmin) ? UserRoles.SuperAdmin :
            User.IsInRole(UserRoles.CompanyHR) ? UserRoles.CompanyHR :
            User.IsInRole(UserRoles.PlacementOfficer) ? UserRoles.PlacementOfficer :
            User.IsInRole(UserRoles.Student) ? UserRoles.Student :
            string.Empty;

        [HttpPost]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Create(CreateRecruitmentDriveDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto, UserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await _service.GetAllAsync(UserId, UserRole));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(
                    id, UserId, UserRole);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.CompanyHR + "," + UserRoles.SuperAdmin)]
        public async Task<IActionResult> Update(
            int id, UpdateRecruitmentDriveDto dto)
        {
            try
            {
                return Ok(await _service.UpdateAsync(
                    id, dto, UserId, UserRole));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = UserRoles.CompanyHR + "," + UserRoles.SuperAdmin)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _service.CancelAsync(id, UserId, UserRole);

                return Ok(new
                {
                    message = "Recruitment drive cancelled successfully."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
