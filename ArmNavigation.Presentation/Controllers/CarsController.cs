using ArmNavigation.Domain.Enums;
using ArmNavigation.Domain.Models;
using ArnNavigation.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArmNavigation.Presentation.Controllers
{
    [ApiController]
    [Route("api/cars")]
    [Authorize]
    public sealed class CarsController : ControllerBase
    {
        private readonly ICarsService _service;

        public CarsController(ICarsService service)
        {
            _service = service;
        }

        private static (int Role, Guid OrgId) GetUserContext(ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            var orgClaim = user.FindFirst("org")?.Value;

            int role = int.TryParse(roleClaim, out var r) ? r : (int)Role.Dispatcher;
            Guid org = Guid.TryParse(orgClaim, out var o) ? o : Guid.Empty;

            return (role, org);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> List([FromQuery] Guid? orgId, CancellationToken token)
        {
            var (role, org) = GetUserContext(User);
            var result = await _service.ListAsync(role, org, orgId, token);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Car?>> Get(Guid id, CancellationToken token)
        {
            var (role, org) = GetUserContext(User);
            var entity = await _service.GetAsync(id, role, org, token);
            if (entity is null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCarRequest request, CancellationToken token)
        {
            var (role, org) = GetUserContext(User);
            var id = await _service.CreateAsync(request.RegNum, request.MedInstitutionId, request.GpsTracker, role, org, token);
            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCarRequest request, CancellationToken token)
        {
            var (role, org) = GetUserContext(User);
            var updated = await _service.UpdateAsync(id, request.RegNum, request.MedInstitutionId, request.GpsTracker, role, org, token);

            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken token)
        {
            var (role, org) = GetUserContext(User);
            var removed = await _service.RemoveAsync(id, role, org, token);

            if (removed is null)
                return NoContent();

            return NotFound();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Car>>> Search([FromQuery] string query, [FromQuery] Guid? orgId, CancellationToken token)
        {
            var (role, org) = GetUserContext(User);
            var result = await _service.GetAsync(query, role, org, orgId, token);
            return Ok(result);
        }

        [HttpPost("bind-tracker/{id:guid}")]
        public async Task<ActionResult> BindTracker(Guid id, [FromBody] BindTrackerRequest request, CancellationToken token)
        {
            var (role, org) = GetUserContext(User);
            var result = await _service.BindTrackerAsync(id, request.Tracker, role, org, token);

            return Ok(result);
        }

        [HttpPost("unbind-tracker/{id:guid}")]
        public async Task<ActionResult> UnbindTracker(Guid id, CancellationToken token)
        {
            var (role, org) = GetUserContext(User);
            var result = await _service.UnbindTrackerAsync(id, role, org, token);

            return Ok(result);
        }
    }
}