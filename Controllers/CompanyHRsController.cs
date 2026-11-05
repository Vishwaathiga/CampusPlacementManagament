using System.Security.Claims;
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.CompanyHR;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/company-hrs")]
    [ApiController]
    [Authorize]
    public class CompanyHRsController : ControllerBase
    {
        private readonly ICompanyHRService _companyHRService;

        public CompanyHRsController(ICompanyHRService companyHRService)
        {
            _companyHRService = companyHRService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(CompanyHRRegisterDto dto)
        {
            try
            {
                var result = await _companyHRService.RegisterAsync(dto);

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

        [HttpGet]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _companyHRService.GetAllAsync());
        }

        [HttpGet("pending")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetPending()
        {
            return Ok(await _companyHRService.GetPendingAsync());
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var approvedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(approvedBy))
                {
                    return Unauthorized();
                }

                return Ok(await _companyHRService.ApproveAsync(id, approvedBy));
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

        [HttpPut("{id}/reject")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                return Ok(await _companyHRService.RejectAsync(id));
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

        [HttpGet("me")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _companyHRService.GetMyProfileAsync(userId);

            if (result == null)
            {
                return NotFound(new { message = "Company HR profile not found." });
            }

            return Ok(result);
        }
    }
}