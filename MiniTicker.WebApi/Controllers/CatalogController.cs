using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Catalogs;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Users;
using MiniTicker.Core.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    [Tags("02. Catálogos")]

    // Se requiere estar autenticado para cualquier acción
    [Authorize]
    public class CatalogController : ControllerBase
    {
        private readonly IAreaService _areaService;
        private readonly ITipoSolicitudService _tipoService;

        private readonly IUserService _userService;

        public CatalogController(IAreaService areaService, ITipoSolicitudService tipoService, IUserService userService)
        {
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _tipoService = tipoService ?? throw new ArgumentNullException(nameof(tipoService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        // ============================
        // ÁREAS
        // ============================

        [HttpGet("areas")]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> GetAreas([FromQuery] bool mostrarInactivos = false, CancellationToken cancellationToken = default)
        {
            var items = await _areaService.GetAllAsync(mostrarInactivos, cancellationToken);
            return Ok(items);
        }

        // Escritura: Solo Admin gestiona catálogos.
        [HttpPost("areas")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreateArea(
              [FromBody] CreateAreaDto dto,
              CancellationToken cancellationToken)
        {
            if (dto == null) return BadRequest();

            // 3. Pasamos el token al servicio
            var created = await _areaService.CreateAsync(dto, cancellationToken).ConfigureAwait(false);

            return CreatedAtAction(nameof(GetAreaById), new { id = created.Id }, created);
        }

        [HttpGet("areas/{id:guid}")]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> GetAreaById(Guid id)
        {
            var area = await _areaService.GetByIdAsync(id).ConfigureAwait(false);
            if (area == null) return NotFound();
            return Ok(area);
        }

        [HttpPut("areas/{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateArea(Guid id, [FromBody] AreaDto dto)
        {
            if (dto == null) return BadRequest();
            var updated = await _areaService.UpdateAsync(id, dto).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpDelete("areas/{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteArea(Guid id)
        {
            await _areaService.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPatch("areas/{id:guid}/activate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ActivateArea(Guid id, CancellationToken cancellationToken)
        {
            await _areaService.ActivateAsync(id, cancellationToken);
            return Ok(new { message = "El área ha sido reactivada exitosamente." });
        }

        [HttpPatch("areas/{id:guid}/desactivate")]
        [Authorize(Roles = "Admin,SuperAdmin")]

        public async Task<IActionResult> DeactivateArea(Guid id, CancellationToken cancellationToken)
        {
            await _areaService.DeactivateAsync(id, cancellationToken);
            return Ok(new { message = "El área ha sido desactivada exitosamente." });
        }

        [HttpPatch("areas/{id:guid}/quitar-responsable/{usuarioId:guid}")]
        public async Task<IActionResult> QuitarResponsable(Guid id, Guid usuarioId)
        {
            try
            {
                await _areaService.QuitarResponsableArea(id, usuarioId);
                return NoContent(); // Retorna 204 si todo salió bien
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // ============================
        // TIPOS DE SOLICITUD
        // ============================


        [HttpGet("tipos-solicitud")]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> GetTipos(
            [FromQuery] Guid? areaId,
            [FromQuery] bool mostrarInactivos = false,
            CancellationToken cancellationToken = default)
        {
            var items = await _tipoService.GetAllAsync(areaId, mostrarInactivos, cancellationToken);
            return Ok(items);
        }


        [HttpPost("tipos-solicitud")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreateTipo([FromBody] TipoSolicitudDto dto)
        {
            if (dto == null) return BadRequest();
            var created = await _tipoService.CreateAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetTipos), new { areaId = created.AreaId }, created);
        }

        [HttpPut("tipos-solicitud/{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateTipo(Guid id, [FromBody] TipoSolicitudDto dto)
        {
            if (dto == null) return BadRequest();
            var updated = await _tipoService.UpdateAsync(id, dto).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpDelete("tipos-solicitud/{id:guid}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteTipo(Guid id)
        {
            await _tipoService.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPatch("tipos-solicitud/{id:guid}/activate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ActivateTipo(Guid id, CancellationToken cancellationToken)
        {
            await _tipoService.ActivateAsync(id, cancellationToken);
            return Ok(new { message = "El registro ha sido reactivado exitosamente." });
        }

        [HttpPatch("tipos-solicitud/{id:guid}/desactivate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeactivateTipo(Guid id, CancellationToken cancellationToken)
        {
            await _tipoService.DeactivateAsync(id, cancellationToken);
            return Ok(new { message = "El registro ha sido desactivado exitosamente." });
        }

        [HttpGet("managers-selection")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetManagersForSelection()
        {
            // Llamamos al servicio que ahora sí filtra por estado activo
            var managers = await _userService.GetActiveManagersAsync();
            return Ok(managers);
        }
        // ============================
        // OTROS
        // ============================

        [HttpGet("prioridades")]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public IActionResult GetPrioridades()
        {
            var items = Enum.GetValues(typeof(Prioridad)).Cast<Prioridad>().Select(p => new { Id = (int)p, Nombre = p.ToString() }).ToList();
            return Ok(items);
        }
    }
}