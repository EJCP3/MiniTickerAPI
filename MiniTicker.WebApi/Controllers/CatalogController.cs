using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Catalogs;
using MiniTicker.Core.Application.Interfaces.Services;
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

        public CatalogController(IAreaService areaService, ITipoSolicitudService tipoService)
        {
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _tipoService = tipoService ?? throw new ArgumentNullException(nameof(tipoService));
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