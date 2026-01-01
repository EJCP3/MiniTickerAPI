using MiniTicker.Core.Application.Actividad;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ITicketEventRepository _eventRepository;

        public ActivityService(ITicketEventRepository eventRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        // ✅ MÉTODO 1: ACTIVIDAD PERSONAL (El que faltaba)
        public async Task<IReadOnlyList<ActivityDto>> GetMyActivityAsync(Guid userId)
        {
            // Usamos el repositorio específico para "Mis actividades"
            var events = await _eventRepository.GetRecentByUserIdAsync(userId);
            var dtos = new List<ActivityDto>();

            foreach (var ev in events)
            {
                bool fuiYo = ev.UsuarioId == userId;
                string nombreActor = fuiYo ? "Tú" : (ev.Usuario?.Nombre ?? "Un gestor");

                dtos.Add(CrearDto(ev, fuiYo, nombreActor));
            }
            return dtos;
        }

        // ✅ MÉTODO 2: ACTIVIDAD GLOBAL (Para Admins/Gestores)
        public async Task<IReadOnlyList<ActivityDto>> GetGlobalActivityAsync(Guid currentUserId, Guid? areaId = null, Guid? targetUserId = null)
        {
            // Usamos el repositorio con filtros globales
            var events = await _eventRepository.GetGlobalRecentAsync(areaId, targetUserId);
            var dtos = new List<ActivityDto>();

            foreach (var ev in events)
            {
                bool fuiYo = ev.UsuarioId == currentUserId;
                string nombreActor = fuiYo ? "Tú" : (ev.Usuario?.Nombre ?? "Un usuario");

                dtos.Add(CrearDto(ev, fuiYo, nombreActor));
            }
            return dtos;
        }

        // --- HELPERS PRIVADOS (Para no repetir código) ---

        private ActivityDto CrearDto(TicketEvent ev, bool fuiYo, string nombreActor)
        {
            return new ActivityDto
            {
                Id = ev.Id,
                TicketId = ev.TicketId,
                Titulo = ev.Ticket != null ? $"Ticket {ev.Ticket.Numero}" : "Sistema",
                Mensaje = FormatearMensaje(ev, fuiYo, nombreActor),
                Fecha = CalcularHaceCuanto(ev.Fecha),
                Tipo = ev.TipoEvento.ToString()
            };
        }

        private string FormatearMensaje(TicketEvent ev, bool fuiYo, string actor)
        {
            return ev.TipoEvento switch
            {
                TicketEventType.Creado => fuiYo
                    ? "Has creado este ticket."
                    : $"{actor} ha creado este ticket a tu nombre.",

                TicketEventType.CambioEstado => fuiYo
                    ? $"Cambiaste el estado a {ev.EstadoNuevo}."
                    : $"{actor} actualizó el estado a {ev.EstadoNuevo}.",

                TicketEventType.ComentarioADD => fuiYo
                    ? "Agregaste un comentario."
                    : $"{actor} respondió: \"{CortarTexto(ev.Texto)}\"",

                TicketEventType.Asignado => fuiYo
                    ? "Te has autoasignado este ticket."
                    : $"{actor} ha tomado tu ticket.",

                _ => "Actividad registrada."
            };
        }

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
            return fechaUtc.ToLocalTime().ToString("dd/MM/yyyy");
        }
    }
}