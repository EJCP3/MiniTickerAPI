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
    [Authorize] 
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketEventRepository _ticketEventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IComentarioRepository _comentarioRepository;

        public TicketController(
            ITicketService ticketService,
            ITicketEventRepository ticketEventRepository,
            IUserRepository userRepository,
            IComentarioRepository comentarioRepository)

        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _ticketEventRepository = ticketEventRepository ?? throw new ArgumentNullException(nameof(ticketEventRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _comentarioRepository = comentarioRepository;
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = default;
            var claimValue = User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(claimValue))
                claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimValue))
                claimValue = User.FindFirst("id")?.Value;

            return !string.IsNullOrEmpty(claimValue) && Guid.TryParse(claimValue, out userId);
        }

        // ============================================================
        // CREACIÓN
        // ============================================================

        // Permitimos a todos los roles autenticados crear tickets.
        [HttpPost]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> Create([FromForm] CreateTicketDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                var claimsLlegaron = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
                return Unauthorized($"No se pudo identificar el usuario. Claims recibidos: {claimsLlegaron}");
            }

            var created = await _ticketService.CreateAsync(dto, userId, cancellationToken);
            return Ok(created);
        }

        // ============================================================
        // LECTURA (LISTADO Y DETALLE)
        // ============================================================


        [HttpGet]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> GetPaged([FromQuery] TicketFilterDto filter, CancellationToken cancellationToken)
        {
            var result = await _ticketService.GetPagedAsync(filter, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("{ticketId:guid}")]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> GetById(Guid ticketId, CancellationToken cancellationToken)
        {
            var detail = await _ticketService.GetByIdAsync(ticketId, cancellationToken).ConfigureAwait(false);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        // ============================================================
        // EDICIÓN Y GESTIÓN
        // ============================================================

        [HttpPut("{ticketId:guid}")]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> Update(Guid ticketId, [FromForm] UpdateTicketDto dto, CancellationToken cancellationToken)
        {
            if (dto == null) return BadRequest();

            var updated = await _ticketService.UpdateAsync(ticketId, dto, cancellationToken).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpPatch("{ticketId:guid}/status")]
        [Authorize(Roles = "Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> ChangeStatus(Guid ticketId, [FromBody] ChangeTicketStatusDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                var claimsLlegaron = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
                return Unauthorized($"No se pudo identificar el usuario. Claims recibidos: {claimsLlegaron}");
            }
            var updated = await _ticketService.ChangeStatusAsync(ticketId, dto, userId, cancellationToken).ConfigureAwait(false);
            return Ok(updated);
        }


        [HttpPatch("{ticketId:guid}/assign")]
        [Authorize(Roles = "Admin,SuperAdmin,Gestor")]
        public async Task<IActionResult> Assign(Guid ticketId, [FromBody] AssignTicketDto dto, CancellationToken cancellationToken)
        {
            if (dto == null) return BadRequest();

            await _ticketService.AssignManagerAsync(ticketId, dto, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        // ============================================================
        // COMENTARIOS E HISTORIAL
        // ============================================================

        [HttpPost("{ticketId:guid}/comentarios")]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> AddComment(
            Guid ticketId,
            [FromBody] CreateComentarioDto dto,
            CancellationToken cancellationToken)
        {
            if (dto == null) return BadRequest("Datos inválidos.");
            if (string.IsNullOrWhiteSpace(dto.Texto)) return BadRequest("El comentario no puede estar vacío.");

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("No se pudo identificar el usuario.");
            }

            var result = await _ticketService.AddCommentAsync(ticketId, dto, userId, cancellationToken);

            return Ok(result);
        }
        [HttpGet("{id:guid}/historial")]
        [Authorize(Roles = "Solicitante,Gestor,Admin,SuperAdmin")]
        public async Task<IActionResult> GetHistorial(Guid id, CancellationToken cancellationToken)
        {
            // 1. Validar ticket
            var ticket = await _ticketService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (ticket == null) return NotFound("El ticket no existe.");

            // 2. Traer EVENTOS (Cambios de estado, asignaciones)
            var eventos = await _ticketEventRepository
                .GetByTicketIdOrderedAscAsync(id, cancellationToken)
                .ConfigureAwait(false);

            // 3. Traer COMENTARIOS (El chat) - ¡ESTO ES LO QUE FALTABA!
            var comentarios = await _comentarioRepository
                .GetByTicketIdOrderedByFechaAscAsync(id); // Asegúrate de que tu repo tenga este método o usa un Where

            var items = new System.Collections.Generic.List<TicketHistoryDto>();

            // --- PROCESAR EVENTOS ---
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
                        if (!string.IsNullOrEmpty(e.Texto)) descripcion = $". Motivo: {e.Texto}";
                        dto.Descripcion = descripcion;
                        break;
                    case Core.Domain.Enums.TicketEventType.ComentarioADD:
                        // Este caso es para comentarios guardados como eventos (si existieran)
                        dto.Titulo = "Comentario agregado";
                        dto.Subtitulo = $"Por: {autorNombreRol}";
                        dto.Descripcion = e.Texto;
                        break;
                    case Core.Domain.Enums.TicketEventType.Asignado:
                        dto.Titulo = "Gestor Asignado";
                        dto.Subtitulo = $"Gestor responsable: {autorNombreRol}";
                        dto.Descripcion = "El ticket ha sido asignado a un gestor para su atención.";
                        break;
                }
                items.Add(dto);
            }

            // --- PROCESAR COMENTARIOS (Hacerlos parecer eventos del historial) ---
            foreach (var c in comentarios)
            {
                var autor = await _userRepository.GetByIdAsync(c.UsuarioId).ConfigureAwait(false);
                var autorNombre = autor?.Nombre ?? "Usuario";

                var dto = new TicketHistoryDto
                {
                    // Usamos el mismo formato de fecha
                    Fecha = c.Fecha.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
                    // Disfrazamos el comentario como un evento de tipo "ComentarioADD"
                    TipoEvento = Core.Domain.Enums.TicketEventType.ComentarioADD,
                    Titulo = "Comentario",
                    Subtitulo = $"Por: {autorNombre}", // Puedes agregar el rol si quieres: $"{autorNombre} ({autor.Rol})"
                    Descripcion = c.Texto, // Aquí va el mensaje del chat
                    VisibleParaSolicitante = true,
                    VisibleSoloGestores = false
                };
                items.Add(dto);
            }

            // 4. ORDENAR TODO CRONOLÓGICAMENTE (Eventos y Comentarios mezclados)
            // Parseamos la fecha string para ordenar correctamente
            items = items.OrderBy(x => DateTime.ParseExact(x.Fecha, "dd/MM/yyyy hh:mm tt", null)).ToList();

            return Ok(items);
        }
    }
}