using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.PlacementOfficer;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusPlacementManagement.Controllers
{
    [ApiController]
    [Route("api/placement-officers")]
    [Authorize]
    public class PlacementOfficersController : ControllerBase
    {
        private readonly IPlacementOfficerService _service;

        public PlacementOfficersController(IPlacementOfficerService service)
        {
            _service = service;
        }

        // Public registration
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            PlacementOfficerRegisterDto dto)
        {
            try
            {
                var result = await _service.RegisterAsync(dto);

                return StatusCode(201, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // SuperAdmin: View pending officers
        [HttpGet("pending")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetPending()
        {
            var result = await _service.GetPendingAsync();

            return Ok(result);
        }

        // SuperAdmin: View all officers
        [HttpGet]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        // SuperAdmin or the officer themselves
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = "Placement officer not found." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole(UserRoles.SuperAdmin))
            {
                var myProfile = await _service.GetMyProfileAsync(userId!);

                if (myProfile == null || myProfile.PlacementOfficerId != id)
                {
                    return Forbid();
                }
            }

            return Ok(result);
        }

        // PlacementOfficer: View own profile
        [HttpGet("me")]
        [Authorize(Roles = UserRoles.PlacementOfficer)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _service.GetMyProfileAsync(userId!);

            if (result == null)
            {
                return NotFound(new { message = "Placement officer profile not found." });
            }

            return Ok(result);
        }

        // SuperAdmin: Approve officer
        [HttpPut("{id}/approve")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var result = await _service.ApproveAsync(id, approverId);

                return Ok(result);
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

        // SuperAdmin: Reject officer
        [HttpPut("{id}/reject")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var result = await _service.RejectAsync(id);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // SuperAdmin: Suspend officer
        [HttpPut("{id}/suspend")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Suspend(int id)
        {
            try
            {
                var result = await _service.SuspendAsync(id);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}