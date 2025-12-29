using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Comments;
using MiniTicker.Core.Application.Filters;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Read;
using MiniTicker.Core.Application.Tickets;
using MiniTicker.Core.Domain.Entities;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketEventRepository _ticketEventRepository;
        private readonly IUserRepository _userRepository;

        public TicketController(
            ITicketService ticketService,
            ITicketEventRepository ticketEventRepository,
            IUserRepository userRepository)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _ticketEventRepository = ticketEventRepository ?? throw new ArgumentNullException(nameof(ticketEventRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = default;

            // 1. Busca el claim "sub" (lo que configuramos)
            var claimValue = User.FindFirst("sub")?.Value;

            // 2. Si no lo encuentra, busca el "NameIdentifier" (el nombre largo por defecto de Microsoft)
            if (string.IsNullOrEmpty(claimValue))
            {
                claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            // 3. Si tampoco, busca por "id" (por si acaso)
            if (string.IsNullOrEmpty(claimValue))
            {
                claimValue = User.FindFirst("id")?.Value;
            }

            // Validamos que sea un Guid válido
            return !string.IsNullOrEmpty(claimValue) && Guid.TryParse(claimValue, out userId);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateTicketDto dto, CancellationToken cancellationToken)
        {
            // Usamos el método robusto
            if (!TryGetUserId(out var userId))
            {
                // TRUCO DE DEBUG:
                // Si falla, devolvemos la lista de lo que SÍ llegó para que veas qué está pasando
                var claimsLlegaron = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
                return Unauthorized($"No se pudo identificar el usuario. Claims recibidos: {claimsLlegaron}");
            }

            var created = await _ticketService.CreateAsync(dto, userId, cancellationToken);
            return Ok(created);
        }


        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] TicketFilterDto filter, CancellationToken cancellationToken)
        {
            var result = await _ticketService.GetPagedAsync(filter, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("{ticketId:guid}")]
        public async Task<IActionResult> GetById(Guid ticketId, CancellationToken cancellationToken)
        {
            var detail = await _ticketService.GetByIdAsync(ticketId, cancellationToken).ConfigureAwait(false);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        [HttpPut("{ticketId:guid}")]
        public async Task<IActionResult> Update(Guid ticketId, [FromForm] UpdateTicketDto dto, CancellationToken cancellationToken)
        {
            if (dto == null) return BadRequest();

            var updated = await _ticketService.UpdateAsync(ticketId, dto, cancellationToken).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpPatch("{ticketId:guid}/status")]
        public async Task<IActionResult> ChangeStatus(Guid ticketId, [FromBody] ChangeTicketStatusDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                // TRUCO DE DEBUG:
                // Si falla, devolvemos la lista de lo que SÍ llegó para que veas qué está pasando
                var claimsLlegaron = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
                return Unauthorized($"No se pudo identificar el usuario. Claims recibidos: {claimsLlegaron}");
            }
            var updated = await _ticketService.ChangeStatusAsync(ticketId, dto, userId, cancellationToken).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpPatch("{ticketId:guid}/assign")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Assign(Guid ticketId, [FromBody] AssignTicketDto dto, CancellationToken cancellationToken)
        {
            if (dto == null) return BadRequest();

            await _ticketService.AssignManagerAsync(ticketId, dto, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPost("{ticketId:guid}/comentarios")]
        [Authorize] // Requiere que el usuario esté logueado
        public async Task<IActionResult> AddComment(
    Guid ticketId,
    [FromBody] CreateComentarioDto dto,
    CancellationToken cancellationToken)
        {
            if (dto == null) return BadRequest("Datos inválidos.");
            if (string.IsNullOrWhiteSpace(dto.Texto)) return BadRequest("El comentario no puede estar vacío.");

            // Usamos tu método robusto para obtener el ID del usuario logueado
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("No se pudo identificar el usuario.");
            }

            var result = await _ticketService.AddCommentAsync(ticketId, dto, userId, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}/historial")]
        public async Task<IActionResult> GetHistorial(Guid id, CancellationToken cancellationToken)


        {

            var ticket = await _ticketService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (ticket == null) return NotFound("El ticket no existe.");

            var eventos = await _ticketEventRepository
                .GetByTicketIdOrderedAscAsync(id, cancellationToken)
                .ConfigureAwait(false);

            var items = new System.Collections.Generic.List<TicketHistoryDto>();
            foreach (var e in eventos)
            {
                var autor = await _userRepository.GetByIdAsync(e.UsuarioId).ConfigureAwait(false);
                var autorNombreRol = autor == null ? "Desconocido" : $"{autor.Nombre}";

                var dto = new TicketHistoryDto
                {
                    Fecha = e.Fecha.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
                    TipoEvento = e.TipoEvento,
                    EstadoAnterior = e.EstadoAnterior,
                    EstadoNuevo = e.EstadoNuevo,
                    TipoComentario = e.TipoComentario,
                    VisibleParaSolicitante = e.VisibleParaSolicitante,
                    VisibleSoloGestores = e.VisibleSoloGestores
                };

                switch (e.TipoEvento)
                {
                    case Core.Domain.Enums.TicketEventType.Creado:
                        dto.Titulo = "Solicitud creada";
                        dto.Subtitulo = $"Por: {autorNombreRol}";
                        dto.Descripcion = $"Se creó la solicitud para {ticket.Asunto}";
                        break;
                    case Core.Domain.Enums.TicketEventType.CambioEstado:
                        dto.Titulo = $"Estado actualizado a {e.EstadoNuevo}";
                        dto.Subtitulo = $"Por: {autorNombreRol}";
                        string descripcion = $"Cambio de estado de {e.EstadoAnterior} a {e.EstadoNuevo}";
                        if (!string.IsNullOrEmpty(e.Texto))
                        {
                            descripcion = $". Motivo: {e.Texto}";
                        }
                        dto.Descripcion = descripcion;
                        break;
                    case Core.Domain.Enums.TicketEventType.ComentarioADD:
                        dto.Titulo = "Comentario agregado";
                        dto.Subtitulo = $"Por:{autorNombreRol}";
                        dto.Descripcion = e.Texto;
                        break;
                    case Core.Domain.Enums.TicketEventType.Asignado:
                        dto.Titulo = "Gestor Asignado";
                        // Como guardamos el ID del gestor en el evento, 'autorNombreRol' será el nombre del gestor
                        dto.Subtitulo = $"Gestor responsable: {autorNombreRol}";
                        dto.Descripcion = "El ticket ha sido asignado a un gestor para su atención.";
                        break;
                }

                items.Add(dto);
            }

            return Ok(items);
        }

        //[HttpGet("check-mis-permisos")]
        //[Authorize]
        //public IActionResult CheckPermissions()
        //{
        //    var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        //    var esSuperAdmin = User.IsInRole("SuperAdmin");
        //    var esAdmin = User.IsInRole("Admin");

        //    return Ok(new
        //    {
        //        Mensaje = "Diagnóstico de permisos",
        //        EsSuperAdmin = esSuperAdmin,
        //        EsAdmin = esAdmin,
        //        MisClaims = claims
        //    });
        //}
    }
}
