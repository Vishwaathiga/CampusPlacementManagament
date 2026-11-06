using System.Security.Claims;
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.Interview;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/jobs/{jobId:int}/interview-rounds")]
    [ApiController]
    [Authorize]
    public class InterviewRoundsController : ControllerBase
    {
        private readonly IInterviewRoundService _service;

        public InterviewRoundsController(IInterviewRoundService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Create(
            int jobId, CreateInterviewRoundDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.CreateAsync(jobId, dto, userId);

            return CreatedAtAction(
                nameof(GetByJobId),
                new { jobId },
                result);
        }

        [HttpGet]
        public async Task<IActionResult> GetByJobId(int jobId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var role = User.FindFirstValue(ClaimTypes.Role)!;

            var result = await _service.GetByJobIdAsync(jobId, userId, role);

            return Ok(result);
        }
    }
}