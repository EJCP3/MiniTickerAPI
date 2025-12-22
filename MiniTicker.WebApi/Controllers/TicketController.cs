using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Filters;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Tickets;
using MiniTicker.Core.Application.Read;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tickets")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateTicketDto dto)
        {
            if (dto == null) return BadRequest();

            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var created = await _ticketService.CreateAsync(dto, userId).ConfigureAwait(false);

            return CreatedAtAction(nameof(GetById), new { ticketId = created.Id }, created);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] TicketFilterDto filter)
        {
            var result = await _ticketService.GetPagedAsync(filter).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("{ticketId:guid}")]
        public async Task<IActionResult> GetById(Guid ticketId)
        {
            var detail = await _ticketService.GetByIdAsync(ticketId).ConfigureAwait(false);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        [HttpPut("{ticketId:guid}")]
        public async Task<IActionResult> Update(Guid ticketId, [FromBody] UpdateTicketDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = await _ticketService.UpdateAsync(ticketId, dto).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpPatch("{ticketId:guid}/status")]
        public async Task<IActionResult> ChangeStatus(Guid ticketId, [FromBody] ChangeTicketStatusDto dto)
        {
            if (dto == null) return BadRequest();

            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var updated = await _ticketService.ChangeStatusAsync(ticketId, dto, userId).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpPatch("{ticketId:guid}/assign")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Assign(Guid ticketId, [FromBody] AssignTicketDto dto)
        {
            if (dto == null) return BadRequest();

            await _ticketService.AssignManagerAsync(ticketId, dto).ConfigureAwait(false);
            return NoContent();
        }
    }
}
