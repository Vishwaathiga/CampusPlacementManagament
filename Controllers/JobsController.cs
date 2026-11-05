using System.Security.Claims;
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.Job;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private string CurrentRole()
        {
            if (User.IsInRole(UserRoles.SuperAdmin))
                return UserRoles.SuperAdmin;

            if (User.IsInRole(UserRoles.CompanyHR))
                return UserRoles.CompanyHR;

            if (User.IsInRole(UserRoles.PlacementOfficer))
                return UserRoles.PlacementOfficer;

            if (User.IsInRole(UserRoles.Student))
                return UserRoles.Student;

            throw new UnauthorizedAccessException();
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Create(CreateJobDto dto)
        {
            try
            {
                var result = await _jobService.CreateAsync(dto, UserId);
                return CreatedAtAction(nameof(GetById),
                    new { id = result.JobId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
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
                var result = await _jobService.GetAllAsync(UserId, CurrentRole());
                return Ok(result);
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
                var result = await _jobService.GetByIdAsync(id, UserId, CurrentRole());

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
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Update(int id, UpdateJobDto dto)
        {
            try
            {
                return Ok(await _jobService.UpdateAsync(id, dto, UserId));
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

        [HttpPut("{id}/publish")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Publish(int id)
        {
            try
            {
                return Ok(await _jobService.PublishAsync(id, UserId));
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

        [HttpPut("{id}/close")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                return Ok(await _jobService.CloseAsync(id, UserId));
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

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _jobService.DeleteAsync(id, UserId);
                return NoContent();
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

        [HttpGet("eligible")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<IActionResult> GetEligibleJobs()
        {
            try
            {
                return Ok(await _jobService.GetEligibleJobsAsync(UserId));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("{id}/eligibility")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<IActionResult> CheckEligibility(int id)
        {
            try
            {
                return Ok(new
                {
                    JobId = id,
                    IsEligible = await _jobService.CheckEligibilityAsync(id, UserId)
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
        }

        [HttpPost("{id}/target-colleges")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> AddTargetColleges(
            int id, TargetCollegesDto dto)
        {
            try
            {
                await _jobService.AddTargetCollegesAsync(id, dto, UserId);
                return Ok(new { message = "Target colleges added successfully." });
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

        [HttpGet("{id}/target-colleges")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> GetTargetColleges(int id)
        {
            try
            {
                return Ok(await _jobService.GetTargetCollegesAsync(id, UserId));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}/target-colleges/{collegeId}")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> RemoveTargetCollege(int id, int collegeId)
        {
            try
            {
                await _jobService.RemoveTargetCollegeAsync(id, collegeId, UserId);
                return NoContent();
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