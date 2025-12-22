using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Catalogs;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tipos-solicitud")]
    public class TipoSolicitudController : ControllerBase
    {
        private readonly ITipoSolicitudService _tipoService;

        public TipoSolicitudController(ITipoSolicitudService tipoService)
        {
            _tipoService = tipoService ?? throw new ArgumentNullException(nameof(tipoService));
        }

        [HttpGet("area/{areaId:guid}")]
        public async Task<IActionResult> GetByArea(Guid areaId)
        {
            // Cambiar la llamada para usar GetAllAsync, ya que GetByAreaIdAsync devuelve void
            var items = await _tipoService.GetAllAsync(areaId).ConfigureAwait(false);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TipoSolicitudDto dto)
        {
            if (dto == null) return BadRequest();

            var created = await _tipoService.CreateAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetByArea), new { areaId = created.AreaId }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TipoSolicitudDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = await _tipoService.UpdateAsync(id, dto).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _tipoService.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
