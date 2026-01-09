using MiniTicker.Core.Application.Actividad;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq; // Necesario para OrderBy y Select
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ITicketEventRepository _eventRepository;
        private readonly ISystemEventRepository _systemEventRepository;

        public ActivityService(ITicketEventRepository eventRepository, ISystemEventRepository systemEventRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _systemEventRepository = systemEventRepository ?? throw new ArgumentNullException(nameof(systemEventRepository));
        }

        // =========================================================================
        // ✅ MÉTODO 1: ACTIVIDAD PERSONAL (Mi historial: Tickets + Mis acciones)
        // =========================================================================
        public async Task<IReadOnlyList<ActivityDto>> GetMyActivityAsync(Guid userId)
        {
            var combinedActivities = new List<(ActivityDto Dto, DateTime Date)>();

            // 1. Obtener eventos de Tickets
            var ticketEvents = await _eventRepository.GetRecentByUserIdAsync(userId);
            foreach (var ev in ticketEvents)
            {
                bool fuiYo = ev.UsuarioId == userId;
                string actor = fuiYo ? "Tú" : (ev.Usuario?.Nombre ?? "Un gestor");
                combinedActivities.Add((CrearDtoTicket(ev, fuiYo, actor), ev.Fecha));
            }

            // 2. Obtener eventos de Sistema (Login, Logout, etc.)
            // Obtenemos los recientes y filtramos en memoria por el usuario actual
            var systemEvents = await _systemEventRepository.GetRecentAsync(50); // Traemos un lote
            var mySystemEvents = systemEvents.Where(e => e.UsuarioId == userId);

            foreach (var ev in mySystemEvents)
            {
                // En "MyActivity" siempre "fuiYo" es true porque filtramos por userId
                combinedActivities.Add((CrearDtoSistema(ev, true, "Tú"), ev.Fecha));
            }

            // 3. Ordenar por fecha descendente y devolver solo los DTOs
            return combinedActivities
                .OrderByDescending(x => x.Date)
                .Select(x => x.Dto)
                .ToList();
        }

        // =========================================================================
        // ✅ MÉTODO 2: ACTIVIDAD GLOBAL (Dashboard Admin/Gestor)
        // =========================================================================
        public async Task<IReadOnlyList<ActivityDto>> GetGlobalActivityAsync(Guid currentUserId, Guid? areaId = null, Guid? targetUserId = null)
        {
            var combinedActivities = new List<(ActivityDto Dto, DateTime Date)>();

            // 1. Obtener eventos de Tickets (Ya soporta filtros de área y usuario)
            var ticketEvents = await _eventRepository.GetGlobalRecentAsync(areaId, targetUserId);
            foreach (var ev in ticketEvents)
            {
                bool fuiYo = ev.UsuarioId == currentUserId;
                string actor = fuiYo ? "Tú" : (ev.Usuario?.Nombre ?? "Usuario");
                combinedActivities.Add((CrearDtoTicket(ev, fuiYo, actor), ev.Fecha));
            }

            // 2. Obtener eventos de Sistema (Solo si NO filtramos por área, ya que SystemEvent es global)
            if (areaId == null)
            {
                var systemEvents = await _systemEventRepository.GetRecentAsync(30);

                foreach (var ev in systemEvents)
                {
                    // Aplicar filtro de usuario si fue solicitado
                    if (targetUserId.HasValue && ev.UsuarioId != targetUserId) continue;

                    bool fuiYo = ev.UsuarioId == currentUserId;
                    string actor = fuiYo ? "Tú" : (ev.Usuario?.Nombre ?? "Sistema");
                    
                    combinedActivities.Add((CrearDtoSistema(ev, fuiYo, actor), ev.Fecha));
                }
            }

            // 3. Ordenar y retornar
            return combinedActivities
                .OrderByDescending(x => x.Date)
                .Select(x => x.Dto)
                .ToList();
        }

        // =========================================================================
        // --- HELPERS (Mapeo y Formato) ---
        // =========================================================================

        private ActivityDto CrearDtoTicket(TicketEvent ev, bool fuiYo, string nombreActor)
        {
            return new ActivityDto
            {
                Id = ev.Id,
                TicketId = ev.TicketId,
                Titulo = ev.Ticket != null ? $"Ticket {ev.Ticket.Numero}" : "Ticket Eliminado",
                Mensaje = FormatearMensajeTicket(ev, fuiYo, nombreActor),
                Fecha = CalcularHaceCuanto(ev.Fecha),
                Tipo = ev.TipoEvento.ToString(), // "ComentarioADD", "EstadoCambio", etc.
                FechaCreacion = ev.Fecha,
            };
        }

        private ActivityDto CrearDtoSistema(SystemEvent ev, bool fuiYo, string nombreActor)
        {
            return new ActivityDto
            {
                Id = ev.Id,
                TicketId = null, // Eventos de sistema no tienen ticket
                Titulo = "Sistema",
                Mensaje = FormatearMensajeSistema(ev, fuiYo, nombreActor),
                Fecha = CalcularHaceCuanto(ev.Fecha),
                Tipo = ev.Tipo.ToString(), // "Login", "AreaCreada", etc.
                FechaCreacion = ev.Fecha,
            };
        }

        // --- FORMATEADORES DE TEXTO ---

        private string FormatearMensajeTicket(TicketEvent ev, bool fuiYo, string actor)
        {
            return ev.TipoEvento switch
            {
                TicketEventType.Creado => fuiYo
                    ? "Has creado este ticket."
                    : $"{actor} ha creado este ticket.",

                TicketEventType.CambioEstado => fuiYo
                    ? $"Cambiaste el estado a {ev.EstadoNuevo}."
                    : $"{actor} actualizó el estado a {ev.EstadoNuevo}.",

                TicketEventType.ComentarioADD => fuiYo
                    ? "Agregaste un comentario."
                    : $"{actor} comentó: \"{CortarTexto(ev.Texto)}\"",

                TicketEventType.Asignado => fuiYo
                    ? "Te has autoasignado este ticket."
                    : $"{actor} ha tomado tu ticket.",

                _ => "Actividad en ticket."
            };
        }

        private string FormatearMensajeSistema(SystemEvent ev, bool fuiYo, string actor)
        {
            // Función local para conjugar verbos
            string Accion(string verboYo, string verboEl) => fuiYo ? verboYo : $"{actor} {verboEl}";

            return ev.Tipo switch
            {
                // SESIÓN
                SystemEventType.Login => fuiYo ? "Iniciaste sesión." : $"{actor} inició sesión.",
                SystemEventType.Logout => fuiYo ? "Cerraste sesión." : $"{actor} cerró sesión.",

                // USUARIOS
                SystemEventType.UsuarioCreado => 
                    $"{Accion("Registraste", "registró")} al usuario: {ev.Detalles}.",
                SystemEventType.UsuarioActualizado => 
                    $"{Accion("Actualizaste", "actualizó")} el perfil de: {ev.Detalles}.",
                SystemEventType.UsuarioEstadoCambio => 
                    $"{Accion("Cambiaste", "cambió")} estado de usuario: {ev.Detalles}.",

                // ÁREAS
                SystemEventType.AreaCreada => 
                    $"{Accion("Creaste", "creó")} el área: {ev.Detalles}.",
                SystemEventType.AreaActualizada => 
                    $"{Accion("Editaste", "editó")} el área: {ev.Detalles}.",
                SystemEventType.AreaEliminada => 
                    $"{Accion("Eliminaste", "eliminó")} el área: {ev.Detalles}.",
                SystemEventType.AreaEstadoCambio => 
                    $"{Accion("Cambiaste", "cambió")} estado de área: {ev.Detalles}.",
                SystemEventType.AreaResponsableQuitar => 
                    $"{Accion("Quitaste", "quitó")} el responsable del área: {ev.Detalles}.",
                // TIPOS DE SOLICITUD
                SystemEventType.TipoSolicitudCreado => 
                    $"{Accion("Creaste", "creó")} el tipo: {ev.Detalles}.",
                SystemEventType.TipoSolicitudEliminado => 
                    $"{Accion("Eliminaste", "eliminó")} el tipo: {ev.Detalles}.",
                SystemEventType.TipoSolicitudEstadoCambio => 
                    $"{Accion("Cambiaste", "cambió")} estado del tipo: {ev.Detalles}.",

                _ => $"{actor} realizó una acción del sistema."
            };
        }

        // --- UTILIDADES ---

        private string CortarTexto(string? texto)
        {
            if (string.IsNullOrEmpty(texto)) return "...";
            return texto.Length > 30 ? texto.Substring(0, 30) + "..." : texto;
        }

        private string CalcularHaceCuanto(DateTime fechaUtc)
        {
            var span = DateTime.UtcNow - fechaUtc;
            if (span.TotalMinutes < 1) return "Hace un momento";
            if (span.TotalMinutes < 60) return $"Hace {(int)span.TotalMinutes} min";
            if (span.TotalHours < 24) return $"Hace {(int)span.TotalHours} h";
            if (span.TotalDays < 7) return $"Hace {(int)span.TotalDays} d";
            
            return fechaUtc.ToLocalTime().ToString("dd/MM/yyyy");
        }
    }
}