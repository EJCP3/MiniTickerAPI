using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims; // Necesario para los claims
using System.Threading.Tasks;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Actividad; // (Opcional, si usas DTOs aquí)


namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    [Route("api/activity")]
    [Tags("04. Actividad")]
    public class ActivityController : ControllerBase
    {

        private readonly IActivityService _activityService;
        private readonly IDashboardService _dashboardService;

        public ActivityController(IActivityService activityService, IDashboardService dashboardService)
        {
            _activityService = activityService;
            _dashboardService = dashboardService;
        }

        // Endpoint anterior (Mi Actividad)
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyActivity()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var items = await _activityService.GetMyActivityAsync(userId);
            return Ok(items);
        }

        [HttpGet("global")]
        [Authorize(Roles = "Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> GetGlobalActivity(
       [FromQuery] Guid? areaId,
       [FromQuery] Guid? userId
   )
        {
            if (!TryGetUserId(out var currentUserId)) return Unauthorized();

            // Pasamos ambos filtros al servicio
            var items = await _activityService.GetGlobalActivityAsync(currentUserId, areaId, userId);

            return Ok(items);
        }

        // Helper para sacar el ID (puedes ponerlo en una clase base si lo usas mucho)
        private bool TryGetUserId(out Guid userId)
        {
            userId = default;
            var claim = User.FindFirst("uid")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out userId);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] string periodo, [FromQuery] Guid? areaId)
        {
            // Pasamos el areaId (puede ser null o un número)
            var result = await _dashboardService.GetStatsAsync(periodo, areaId);
            return Ok(result);
        }
    }

}