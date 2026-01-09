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
    [Tags("03. Tickets")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketEventRepository _ticketEventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IComentarioRepository _comentarioRepository;

        private readonly ITicketRepository _ticketRepository;

        public TicketController(
            ITicketService ticketService,
            ITicketEventRepository ticketEventRepository,
            IUserRepository userRepository,
            IComentarioRepository comentarioRepository,
            ITicketRepository ticketRepository
            )

        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _ticketEventRepository = ticketEventRepository ?? throw new ArgumentNullException(nameof(ticketEventRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _comentarioRepository = comentarioRepository ?? throw new ArgumentNullException(nameof(comentarioRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
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
            // 1. Obtenemos el ID del usuario actual
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("No se pudo identificar al usuario logueado.");
            }

            // 2. Aplicamos restricciones según el Rol
            if (User.IsInRole("Solicitante"))
            {
                // El solicitante SOLO ve lo que él creó
                filter.UsuarioId = userId;
            }
            else if (User.IsInRole("Gestor"))
            {
                // El Gestor SOLO ve los tickets de su área asignada
                var usuarioActivo = await _userRepository.GetByIdAsync(userId);

                if (usuarioActivo != null && usuarioActivo.AreaId.HasValue)
                {
                    // Forzamos el filtro por su AreaId. 
                    // Esto sobrescribe cualquier intento de buscar otra área desde el frontend.
                    filter.AreaId = usuarioActivo.AreaId.Value;
                }
            }

            // Los roles Admin y SuperAdmin no entran en los IF anteriores, 
            // por lo que conservan el acceso total y respetan los filtros que envíen.

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

        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetStatusSummaryAsync([FromQuery] TicketFilterDto filter)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            // Aplicar lógica de seguridad igual que en el listado
            if (User.IsInRole("Solicitante")) filter.UsuarioId = userId;
            else if (User.IsInRole("Gestor"))
            {
                var gestor = await _userRepository.GetByIdAsync(userId);
                if (gestor?.AreaId != null) filter.AreaId = gestor.AreaId;
            }

            // ✅ Ahora 'summary' no es void
            var summary = await _ticketRepository.GetStatusSummaryAsync(filter);
            return Ok(summary);
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
            var ticket = await _ticketService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (ticket == null) return NotFound("El ticket no existe.");

            var eventos = await _ticketEventRepository
                .GetByTicketIdOrderedAscAsync(id, cancellationToken)
                .ConfigureAwait(false);

            var comentarios = await _comentarioRepository
                .GetByTicketIdOrderedByFechaAscAsync(id);
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
                        if (!string.IsNullOrEmpty(e.Texto)) descripcion = $"Motivo: {e.Texto}";
                        dto.Descripcion = descripcion;
                        break;
                    case Core.Domain.Enums.TicketEventType.ComentarioADD:

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

            foreach (var c in comentarios)
            {
                var autor = await _userRepository.GetByIdAsync(c.UsuarioId).ConfigureAwait(false);
                var autorNombre = autor?.Nombre ?? "Usuario";

                var dto = new TicketHistoryDto
                {
                    Fecha = c.Fecha.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
                    TipoEvento = Core.Domain.Enums.TicketEventType.ComentarioADD,
                    Titulo = "Comentario",
                    Subtitulo = $"Por: {autorNombre}",
                    Descripcion = c.Texto,
                    VisibleParaSolicitante = true,
                    VisibleSoloGestores = false
                };
                items.Add(dto);
            }

            items = items.OrderBy(x => DateTime.ParseExact(x.Fecha, "dd/MM/yyyy hh:mm tt", null)).ToList();

            return Ok(items);
        }
    }
}