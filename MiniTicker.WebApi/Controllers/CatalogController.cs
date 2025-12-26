using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Catalogs;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    
    [Route("api/catalog")]
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
        public async Task<IActionResult> GetAreas()
        {
            var items = await _areaService.GetAllAsync().ConfigureAwait(false);
            return Ok(items);
        }

        [HttpPost("areas")]
        public async Task<IActionResult> CreateArea([FromBody] AreaDto dto)
        {
            if (dto == null) return BadRequest();

            var created = await _areaService.CreateAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetAreaById), new { id = created.Id }, created);
        }

        [HttpGet("areas/{id:guid}")]
        public async Task<IActionResult> GetAreaById(Guid id)
        {
            var area = await _areaService.GetByIdAsync(id).ConfigureAwait(false);
            if (area == null) return NotFound();
            return Ok(area);
        }

        [HttpPut("areas/{id:guid}")]
        public async Task<IActionResult> UpdateArea(Guid id, [FromBody] AreaDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = await _areaService.UpdateAsync(id, dto).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpDelete("areas/{id:guid}")]
        public async Task<IActionResult> DeleteArea(Guid id)
        {
            await _areaService.DeactivateAsync(id).ConfigureAwait(false);
            return NoContent();
        }

        // ============================
        // TIPOS DE SOLICITUD
        // ============================
        [HttpGet("tipos-solicitud/area/{areaId:guid}")]
        public async Task<IActionResult> GetTiposByArea(Guid areaId)
        {
            // Cambiar la llamada para usar GetAllAsync, ya que GetByAreaIdAsync devuelve void
            var items = await _tipoService.GetAllAsync(areaId).ConfigureAwait(false);
            return Ok(items);
        }

        [HttpPost("tipos-solicitud")]
        public async Task<IActionResult> CreateTipo([FromBody] TipoSolicitudDto dto)
        {
            if (dto == null) return BadRequest();

            var created = await _tipoService.CreateAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetTiposByArea), new { areaId = created.AreaId }, created);
        }

        [HttpPut("tipos-solicitud/{id:guid}")]
        public async Task<IActionResult> UpdateTipo(Guid id, [FromBody] TipoSolicitudDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = await _tipoService.UpdateAsync(id, dto).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpDelete("tipos-solicitud/{id:guid}")]
        public async Task<IActionResult> DeleteTipo(Guid id)
        {
            await _tipoService.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }

          [HttpGet("prioridades")]
        public IActionResult GetPrioridades()
        {
            var items = Enum.GetValues(typeof(Prioridad))
                .Cast<Prioridad>()
                .Select(p => new
                {
                    Id = (int)p,
                    Nombre = p.ToString()
                })
                .ToList();

            return Ok(items);
        }
    }
}