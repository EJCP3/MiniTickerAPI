using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Catalogs;
using MiniTicker.Core.Application.Interfaces.Services;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/areas")]
    public class AreaController : ControllerBase
    {
        private readonly IAreaService _areaService;

        public AreaController(IAreaService areaService)
        {
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _areaService.GetAllAsync().ConfigureAwait(false);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AreaDto dto)
        {
            if (dto == null) return BadRequest();

            var created = await _areaService.CreateAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var area = await _areaService.GetByIdAsync(id).ConfigureAwait(false);
            if (area == null) return NotFound();
            return Ok(area);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AreaDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = await _areaService.UpdateAsync(id, dto).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _areaService.DeactivateAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
