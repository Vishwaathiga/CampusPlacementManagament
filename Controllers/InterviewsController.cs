using System.Security.Claims;
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.Interview;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class InterviewsController : ControllerBase
    {
        private readonly IInterviewScheduleService _service;

        public InterviewsController(IInterviewScheduleService service)
        {
            _service = service;
        }

        [HttpPost("applications/{applicationId:int}/interviews")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Create(
            int applicationId, CreateInterviewScheduleDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.CreateAsync(
                applicationId, dto, userId);

            return Ok(result);
        }

        [HttpGet("applications/{applicationId:int}/interviews")]
        public async Task<IActionResult> GetByApplicationId(int applicationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var role = User.FindFirstValue(ClaimTypes.Role)!;

            var result = await _service.GetByApplicationIdAsync(
                applicationId, userId, role);

            return Ok(result);
        }

        [HttpPut("interviews/{id:int}")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> Update(
            int id, UpdateInterviewScheduleDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.UpdateAsync(id, dto, userId);

            return Ok(result);
        }

        [HttpPut("interviews/{id:int}/feedback")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> AddFeedback(
            int id, InterviewFeedbackDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.AddFeedbackAsync(id, dto, userId);

            return Ok(result);
        }
    }
}