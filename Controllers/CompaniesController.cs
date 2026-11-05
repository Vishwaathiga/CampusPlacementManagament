using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs.Company;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(CompanyRegisterDto dto)
        {
            var result = await _companyService.RegisterAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.CompanyId }, result);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _companyService.GetAllAsync());
        }

        [HttpGet("pending")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetPending()
        {
            return Ok(await _companyService.GetPendingAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _companyService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = "Company not found." });
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Update(int id, CompanyUpdateDto dto)
        {
            try
            {
                return Ok(await _companyService.UpdateAsync(id, dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                return Ok(await _companyService.ApproveAsync(id));
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
                return Ok(await _companyService.RejectAsync(id));
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

        [HttpPut("{id}/suspend")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Suspend(int id)
        {
            try
            {
                return Ok(await _companyService.SuspendAsync(id));
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