using MiniTicker.Core.Application.Comments;
using MiniTicker.Core.Application.Filters;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Read;
using MiniTicker.Core.Application.Shared;
using MiniTicker.Core.Application.Tickets;
using MiniTicker.Core.Application.Users;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Runtime.Serialization;

namespace MiniTicker.Core.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IAreaRepository _areaRepository;
        private readonly ITipoSolicitudRepository _tipoSolicitudRepository;
        private readonly IComentarioRepository _comentarioRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ITicketEventRepository _ticketEventRepository;
        private readonly IHttpContextAccessor _httpContextAccessor; // <--- NUEVO

        public TicketService(
            ITicketRepository ticketRepository,
            IAreaRepository areaRepository,
            ITipoSolicitudRepository tipoSolicitudRepository,
            IComentarioRepository comentarioRepository,
            IUserRepository userRepository,
            IFileStorageService fileStorageService,
            ITicketEventRepository ticketEventRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _ticketRepository = ticketRepository;
            _areaRepository = areaRepository;
            _tipoSolicitudRepository = tipoSolicitudRepository;
            _comentarioRepository = comentarioRepository;
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
            _ticketEventRepository = ticketEventRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        // =====================================================
        // CREATE
        // =====================================================
        public async Task<TicketDto> CreateAsync(
              CreateTicketDto dto,
              Guid userId,
              CancellationToken cancellationToken = default)
        {
            // 1. CAMBIO: Guardamos el área en la variable 'area' (antes era "_ =")
            var area = await _areaRepository.GetByIdAsync(dto.AreaId)
                ?? throw new KeyNotFoundException("Área no encontrada.");

            _ = await _tipoSolicitudRepository.GetByIdAsync(dto.TipoSolicitudId)
                ?? throw new KeyNotFoundException("Tipo de solicitud no encontrado.");

            var year = DateTime.UtcNow.Year;
            var (_, totalCount) = await _ticketRepository.GetPagedAsync(new TicketFilterDto());

            // 2. CAMBIO: Usamos el Prefijo del área. Si está vacío, usamos "SOL" por defecto.
            string prefijo = string.IsNullOrWhiteSpace(area.Prefijo) ? "SOL" : area.Prefijo;

            // 3. CAMBIO: Insertamos el prefijo en el formato del número
            var numero = $"{prefijo}-{year}-{totalCount + 1:0000}";

            string? archivoUrl = null;
            if (dto.ArchivoAdjunto != null)
            {
                archivoUrl = await _fileStorageService.UploadAsync(dto.ArchivoAdjunto, "tickets");
            }

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Numero = numero, // Aquí ya va con el prefijo (Ej: SOP-2025-0005)
                Asunto = dto.Asunto,
                Descripcion = dto.Descripcion,
                Prioridad = dto.Prioridad,
                Estado = EstadoTicket.Nueva,
                AreaId = dto.AreaId,
                TipoSolicitudId = dto.TipoSolicitudId,
                SolicitanteId = userId,
                ArchivoAdjuntoUrl = archivoUrl,
                FechaCreacion = DateTime.UtcNow
            };

            await _ticketRepository.AddAsync(ticket);

            await _ticketEventRepository.AddAsync(new TicketEvent
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UsuarioId = userId,
                TipoEvento = TicketEventType.Creado,
                Fecha = DateTime.UtcNow
            }, cancellationToken);

            return await MapToTicketDtoAsync(ticket);
        }

        // =====================================================
        // UPDATE
        // =====================================================
        public async Task<TicketDto> UpdateAsync(
            Guid ticketId,
            UpdateTicketDto dto,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket no encontrado.");

            if (ticket.Estado != EstadoTicket.Nueva)
                throw new InvalidOperationException("Solo se pueden editar tickets en estado Nueva.");

            ticket.Asunto = dto.Asunto;
            ticket.Descripcion = dto.Descripcion;
            ticket.Prioridad = dto.Prioridad;
            ticket.FechaActualizacion = DateTime.UtcNow;
            if (dto.ArchivoAdjunto != null)
            {

                ticket.ArchivoAdjuntoUrl = await _fileStorageService
                    .UploadAsync(dto.ArchivoAdjunto, "tickets");
            }

            // 3. Guardamos los cambios
            await _ticketRepository.UpdateAsync(ticket);

            await _ticketRepository.UpdateAsync(ticket);

            return await MapToTicketDtoAsync(ticket);
        }

        // =====================================================
        // CHANGE STATUS
        // =====================================================
        public async Task<TicketDto> ChangeStatusAsync(
            Guid ticketId,
            ChangeTicketStatusDto dto,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket no encontrado.");

            var estadoAnterior = ticket.Estado;

            if ((int)dto.Estado <= (int)ticket.Estado)
            {
                throw new InvalidOperationException("No es posible retroceder el ticket a un estado anterior.");
            }

            // Si el ticket ya está en un estado final (Cerrada o Rechazada), bloquear cambios
            if (ticket.Estado == EstadoTicket.Cerrada || ticket.Estado == EstadoTicket.Rechazada )
            {
                throw new InvalidOperationException("No se pueden realizar cambios en un ticket finalizado.");
            }

            if (dto.Estado == EstadoTicket.Rechazada && string.IsNullOrWhiteSpace(dto.Motivo))
                throw new InvalidOperationException("Debe indicar el motivo del rechazo.");

            if (dto.Estado == EstadoTicket.Cerrada)
            {
                var user = await _userRepository.GetByIdAsync(userId)
                    ?? throw new KeyNotFoundException("Usuario no encontrado.");

                if (user.Rol is not (Rol.Admin or Rol.SuperAdmin))
                    throw new UnauthorizedAccessException("No tiene permisos para cerrar el ticket.");
            }

            ticket.Estado = dto.Estado;
            ticket.FechaActualizacion = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);

            // Comentario (si aplica)
            if (!string.IsNullOrWhiteSpace(dto.Motivo))
            {
                await _comentarioRepository.AddAsync(new Comentario
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticket.Id,
                    UsuarioId = userId,
                    Texto = dto.Motivo,
                    Fecha = DateTime.UtcNow
                }, cancellationToken);
            }

            // Evento historial
            await _ticketEventRepository.AddAsync(new TicketEvent
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UsuarioId = userId,
                TipoEvento = TicketEventType.CambioEstado,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = dto.Estado,
                Fecha = DateTime.UtcNow,
                Texto = dto.Motivo,
            }, cancellationToken);

            return await MapToTicketDtoAsync(ticket);
        }

        // =====================================================
        // ASSIGN MANAGER
        // =====================================================
        public async Task AssignManagerAsync(
            Guid ticketId,
            AssignTicketDto dto,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket no encontrado.");

            ticket.GestorAsignadoId = dto.GestorId;
            ticket.FechaActualizacion = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);

            await _ticketEventRepository.AddAsync(new TicketEvent
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                UsuarioId = dto.GestorId,
                TipoEvento = TicketEventType.Asignado,
                Fecha = DateTime.UtcNow
            }, cancellationToken);
        }

        // =====================================================
        // PAGED LIST
        // =====================================================
        public async Task<PagedResultDto<TicketDto>> GetPagedAsync(
    TicketFilterDto filter,
    CancellationToken cancellationToken = default)
        {
            // El repositorio ya recibe el objeto 'filter'. 
            // Asegúrate de que tu TicketRepository.GetPagedAsync use el campo filter.UsuarioId 
            // para hacer un: .Where(t => t.SolicitanteId == filter.UsuarioId) si tiene valor.

            var (tickets, total) = await _ticketRepository.GetPagedAsync(filter);

            var items = new List<TicketDto>();
            foreach (var ticket in tickets)
            {
                // Usamos tu helper existente para que cada ticket 
                // incluya la URL de la foto, solicitante, área, etc.
                items.Add(await MapToTicketDtoAsync(ticket));
            }

            return new PagedResultDto<TicketDto>
            {
                Items = items,
                Total = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        // =====================================================
        // DETAIL
        // =====================================================
        public async Task<TicketDetailDto?> GetByIdAsync(
      Guid ticketId,
      CancellationToken cancellationToken = default)
        {
            // 1. El repositorio trae el ticket con sus Eventos (gracias al Include que pusimos antes)
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null) return null;

            // ... (Cargas de Area, Tipo, Solicitante, Gestor siguen igual) ...
            var area = await _areaRepository.GetByIdAsync(ticket.AreaId);
            var tipo = await _tipoSolicitudRepository.GetByIdAsync(ticket.TipoSolicitudId);
            var solicitante = await _userRepository.GetByIdAsync(ticket.SolicitanteId);
            var gestor = ticket.GestorAsignadoId.HasValue
                ? await _userRepository.GetByIdAsync(ticket.GestorAsignadoId.Value)
                : null;

            // ... (Carga de Comentarios sigue igual) ...
            var comentarios = await _comentarioRepository.GetByTicketIdOrderedByFechaAscAsync(ticket.Id);


            // =========================================================================
            // 👇 AQUÍ ESTÁ EL "MAZAZO": CONSTRUIR EL HISTORIAL UNIFICADO
            // =========================================================================
            var historialUnificado = new List<TicketHistoryDto>();

            // A. Agregar Eventos del Sistema (Cambios de estado, asignaciones, creación)
            if (ticket.TicketEvents != null)
            {
                foreach (var evt in ticket.TicketEvents)
                {
                    // Nota: Si 'evt.Usuario' es null, intentamos buscar el nombre, 
                    // o ponemos "Sistema" por defecto.
                    var nombreUsuario = "Sistema";
                    // Si hiciste .Include(t => t.TicketEvents.Select(e => e.Usuario)) en el repo, 
                    // podrías acceder a evt.Usuario.Nombre. Si no, usa el ID o déjalo genérico.

                    var item = new TicketHistoryDto
                    {
                        // Importante: Formato de fecha igual al Frontend
                        Fecha = evt.Fecha.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
                        TipoEvento = evt.TipoEvento,
                        Descripcion = evt.Texto, // El motivo o texto del evento
                        Titulo = evt.TipoEvento.ToString(), // Valor por defecto
                        Subtitulo = ""
                    };

                    // Personalizar textos según el evento (Lógica que tenías en el Controller)
                    switch (evt.TipoEvento)
                    {
                        case TicketEventType.Creado:
                            item.Titulo = "Ticket Creado";
                            item.Subtitulo = $"Solicitante: {solicitante?.Nombre ?? "Usuario"}";
                            if (string.IsNullOrEmpty(item.Descripcion)) item.Descripcion = "Se ha registrado la solicitud.";
                            break;

                        case TicketEventType.CambioEstado:
                            item.Titulo = $"Cambio a {evt.EstadoNuevo}";
                            item.Descripcion = string.IsNullOrEmpty(evt.Texto)
                                ? $"Estado cambiado de {evt.EstadoAnterior} a {evt.EstadoNuevo}"
                                : evt.Texto;
                            break;

                        case TicketEventType.Asignado:
                            item.Titulo = "Gestor Asignado";
                            // Si tienes el nombre del gestor en el evento o en el ticket, úsalo
                            item.Descripcion = "Se asignó un responsable para atender el ticket.";
                            break;

                        case TicketEventType.ComentarioADD:
                            // Los comentarios los tratamos abajo, así que aquí podemos ignorar 
                            // o agregar lógica especial si duplicas datos.
                            item.Titulo = "Comentario Agregado";
                            break;
                    }
                    historialUnificado.Add(item);
                }
            }

            // B. Agregar Comentarios al Historial (para que salgan en la línea de tiempo)
            foreach (var c in comentarios)
            {
                // Opcional: obtener nombre del usuario del comentario si no viene cargado
                // var userCom = await _userRepository.GetByIdAsync(c.UsuarioId); 

                historialUnificado.Add(new TicketHistoryDto
                {
                    Fecha = c.Fecha.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
                    TipoEvento = TicketEventType.ComentarioADD,
                    Titulo = "Comentario",
                    Subtitulo = "Usuario", // O el nombre real si lo buscas
                    Descripcion = c.Texto
                });
            }

            // C. Ordenar: Lo más reciente primero (Descending) o antiguo primero (Ascending)
            // Para historial suele ser Descending (lo nuevo arriba)
            var historialOrdenado = historialUnificado
                .OrderByDescending(h => DateTime.ParseExact(h.Fecha, "dd/MM/yyyy hh:mm tt", null))
                .ToList();


            // =========================================================================
            // RETORNO FINAL
            // =========================================================================
            return new TicketDetailDto
            {
                Id = ticket.Id,
                Numero = ticket.Numero,
                Asunto = ticket.Asunto,
                Descripcion = ticket.Descripcion,
                Estado = ticket.Estado.ToString(),
                Prioridad = ticket.Prioridad.ToString(),
                Area = MapArea(area),
                TipoSolicitud = MapTipo(tipo),
                Solicitante = MapUser(solicitante),
                Gestor = gestor != null ? MapUser(gestor) : null,
                ArchivoAdjuntoUrl = ticket.ArchivoAdjuntoUrl, // (O tu lógica de URL completa)
                FechaCreacion = ticket.FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
                FechaActualizacion = (ticket.FechaActualizacion ?? ticket.FechaCreacion)
                                      .ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),

                Comentarios = comentarios.Select(c => new ComentarioDto
                {
                    Usuario = MapUser(_userRepository.GetByIdAsync(c.UsuarioId).Result),
                    Texto = c.Texto,
                    Fecha = c.Fecha.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
                }).ToList(),

                // 👇 ¡ESTA ES LA LÍNEA QUE FALTABA!
                Historial = historialOrdenado
            };
        }



        public async Task<ComentarioDto> AddCommentAsync(
    Guid ticketId,
    CreateComentarioDto dto,
    Guid userId,
    CancellationToken cancellationToken = default)
        {
            // 1. Validar que el ticket exista
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket no encontrado.");

            // 2. Crear la entidad Comentario
            var comentario = new Comentario
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UsuarioId = userId,
                Texto = dto.Texto,
                Fecha = DateTime.UtcNow
            };

            ticket.FechaActualizacion = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket);

            // 3. Guardar en base de datos
            await _comentarioRepository.AddAsync(comentario, cancellationToken);

            // 4. Registrar el evento en el historial (Audit Log)
            await _ticketEventRepository.AddAsync(new TicketEvent
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UsuarioId = userId,
                TipoEvento = TicketEventType.ComentarioADD,
                Texto = dto.Texto, // Guardamos el texto también en el evento para acceso rápido
                Fecha = DateTime.UtcNow
            }, cancellationToken);

            // 5. Retornar DTO
            var usuario = await _userRepository.GetByIdAsync(userId);

            return new ComentarioDto
            {
                Texto = comentario.Texto,
                Fecha = comentario.Fecha.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
                Usuario = MapUser(usuario) // Usamos tu helper existente MapUser
            };
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private async Task<TicketDto> MapToTicketDtoAsync(Ticket ticket)
        {
            var area = await _areaRepository.GetByIdAsync(ticket.AreaId);
            var tipo = await _tipoSolicitudRepository.GetByIdAsync(ticket.TipoSolicitudId);
            var solicitante = await _userRepository.GetByIdAsync(ticket.SolicitanteId);
            // 1. NUEVO: Buscamos al solicitante por su ID
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";
            string? fullUrl = null;
            if (!string.IsNullOrEmpty(ticket.ArchivoAdjuntoUrl))
            {
                // Resultado: http://localhost:5232/uploads/tickets/archivo.pdf
                fullUrl = $"{baseUrl}/{ticket.ArchivoAdjuntoUrl.Replace("\\", "/")}";
            }
            Usuario? gestor = null;
            if (ticket.GestorAsignadoId.HasValue)
            {
                gestor = await _userRepository.GetByIdAsync(ticket.GestorAsignadoId.Value);
            }

            return new TicketDto
            {
                Id = ticket.Id,
                Numero = ticket.Numero,
                Asunto = ticket.Asunto,
                Estado = ticket.Estado.ToString(),
                Prioridad = ticket.Prioridad.ToString(),
                Area = MapArea(area),
                TipoSolicitud = MapTipo(tipo),
                ArchivoAdjuntoUrl = fullUrl,
                // 2. NUEVO: Asignamos el objeto Solicitante
                // Asegúrate de que tu clase TicketDto tenga esta propiedad: public UserDto Solicitante { get; set; }
                Solicitante = MapUser(solicitante),
                Gestor = gestor != null ? MapUser(gestor) : null,
                FechaCreacion = ticket.FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt"),
            };
        }

        private static Catalogs.AreaDto MapArea(Area? area) =>
            area == null
                ? new() { Id = Guid.Empty, Nombre = string.Empty }
                : new() { Id = area.Id, Nombre = area.Nombre, Activo = area.Activo };

        private static Catalogs.TipoSolicitudDto MapTipo(TipoSolicitud? tipo) =>
            tipo == null
                ? new() { Id = Guid.Empty, Nombre = string.Empty }
                : new() { Id = tipo.Id, Nombre = tipo.Nombre, AreaId = tipo.AreaId, Activo = tipo.Activo };

        private static UserDto MapUser(Usuario? user) =>
            user == null
                ? new() { Id = Guid.Empty, Nombre = string.Empty, Email = string.Empty }
                : new()
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Email = user.Email,
                    Rol = user.Rol.ToString(),
                    FotoPerfilUrl = user.FotoPerfilUrl
                };
    }
}
