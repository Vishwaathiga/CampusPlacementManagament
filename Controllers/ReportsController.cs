using System.Security.Claims;
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/reports")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet("platform-overview")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetPlatformOverview()
        {
            var result = await _reportsService.GetPlatformOverviewAsync();
            return Ok(result);
        }

        [HttpGet("college-overview")]
        [Authorize(Roles = UserRoles.PlacementOfficer)]
        public async Task<IActionResult> GetCollegeOverview()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _reportsService.GetCollegeOverviewAsync(userId);

            return Ok(result);
        }

        [HttpGet("company-recruitment")]
        [Authorize(Roles = UserRoles.CompanyHR)]
        public async Task<IActionResult> GetCompanyRecruitment()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _reportsService.GetCompanyRecruitmentAsync(userId);

            return Ok(result);
        }

        [HttpGet("student-placement")]
        [Authorize(Roles = UserRoles.PlacementOfficer)]
        public async Task<IActionResult> GetStudentPlacement()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _reportsService.GetStudentPlacementAsync(userId);

            return Ok(result);
        }
    }
}