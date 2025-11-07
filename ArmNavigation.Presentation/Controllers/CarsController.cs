using ArmNavigation.Domain.Enums;
using ArmNavigation.Domain.Models;
using ArnNavigation.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArmNavigation.Presentation.Controllers
{
    [ApiController]
    [Route("api/cars")]
    [Authorize]
    public sealed class CarsController : ControllerBase
    {
        private readonly ICarsService _service;
        private readonly UserRoleInfo _currentUser;

        public CarsController(ICarsService service, UserRoleInfo currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<IEnumerable<Car>>>> List([FromQuery] Guid? orgId, CancellationToken token)
        {
            var result = await _service.ListAsync(_currentUser.Role, _currentUser.MedInstitutionId, orgId, token);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseModel<Car>>> Get(Guid id, CancellationToken token)
        {
            var entity = await _service.GetAsync(id, _currentUser.Role, _currentUser.MedInstitutionId, token);
            return entity is null ? NotFound() : Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<Guid>>> Create([FromBody] CreateCarRequest request, CancellationToken token)
        {
            var id = await _service.CreateAsync(
                request.RegNum,
                request.MedInstitutionId,
                request.GpsTracker,
                _currentUser.Role,
                _currentUser.MedInstitutionId,
                token);

            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ResponseModel>> Update(Guid id, [FromBody] UpdateCarRequest request, CancellationToken token)
        {
            var success = await _service.UpdateAsync(
                id,
                request.RegNum,
                request.MedInstitutionId,
                request.GpsTracker,
                _currentUser.Role,
                _currentUser.MedInstitutionId,
                token);

            return (success != null) ? Ok(success) : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResponseModel>> Delete(Guid id, CancellationToken token)
        {
            var success = await _service.RemoveAsync(id, _currentUser.Role, _currentUser.MedInstitutionId, token);
            return success != null ? Ok(success) : NotFound();
        }

        [HttpGet("get")]
        public async Task<ActionResult<ResponseModel<IEnumerable<Car>>>> GetCars([FromQuery] string query, [FromQuery] Guid? orgId, CancellationToken token)
        {
            var result = await _service.GetAsync(query, _currentUser.Role, _currentUser.MedInstitutionId, orgId, token);
            return Ok(result);
        }

        [HttpPost("bind-tracker/{id:guid}")]
        public async Task<ActionResult<ResponseModel>> BindTracker(Guid id, [FromBody] BindTrackerRequest request, CancellationToken token)
        {
            var success = await _service.BindTrackerAsync(id, request.Tracker, _currentUser.Role, _currentUser.MedInstitutionId, token);
            return success != null ? Ok(success) : NotFound();
        }

        [HttpPost("unbind-tracker/{id:guid}")]
        public async Task<ActionResult<ResponseModel>> UnbindTracker(Guid id, CancellationToken token)
        {
            var success = await _service.UnbindTrackerAsync(id, _currentUser.Role, _currentUser.MedInstitutionId, token);
            return success != null ? Ok(success) : NotFound();
        }
    }
}