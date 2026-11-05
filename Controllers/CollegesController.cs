using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.College;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollegesController : ControllerBase
    {
        private readonly ICollegeService _collegeService;

        public CollegesController(ICollegeService collegeService)
        {
            _collegeService = collegeService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(CollegeRegisterDto request)
        {
            try
            {
                var college = await _collegeService.RegisterAsync(request);
                return CreatedAtAction(nameof(GetById),
                    new { id = college.CollegeId }, college);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.SuperAdmin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _collegeService.GetAllAsync());
        }

        [Authorize(Roles = UserRoles.SuperAdmin)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var college = await _collegeService.GetByIdAsync(id);

            if (college == null)
                return NotFound(new { message = "College not found." });

            return Ok(college);
        }

        [Authorize(Roles = UserRoles.SuperAdmin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CollegeUpdateDto request)
        {
            try
            {
                var college = await _collegeService.UpdateAsync(id, request);

                if (college == null)
                    return NotFound(new { message = "College not found." });

                return Ok(college);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.SuperAdmin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _collegeService.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = "College not found." });

            return Ok(new { message = "College deleted successfully." });
        }

        [Authorize(Roles = UserRoles.SuperAdmin)]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id, CollegeApprovalDto request)
        {
            var college = await _collegeService.UpdateStatusAsync(id, request);

            if (college == null)
                return NotFound(new { message = "College not found." });

            return Ok(college);
        }
    }
}