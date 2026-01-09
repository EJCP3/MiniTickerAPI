using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Application.Catalogs;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Services
{
    public class TipoSolicitudService : ITipoSolicitudService
    {
        private readonly ITipoSolicitudRepository _repository;
        private readonly ITicketRepository _ticketRepository; 

        private readonly ISystemEventRepository _eventRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TipoSolicitudService(ITipoSolicitudRepository repository, ITicketRepository ticketRepository, ISystemEventRepository eventRepository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        // ============================================================
        // GET ALL (Fusionado: Maneja Filtro por Área y Filtro de Inactivos)
        // ============================================================
        public async Task<IReadOnlyList<TipoSolicitudDto>> GetAllAsync(
              Guid? areaId = null,
              bool incluirInactivos = false,
              CancellationToken cancellationToken = default)
        {
            // 1. Obtenemos la lista base
            var entities = await _repository.GetAllAsync(incluirInactivos);

            // 2. Filtro en memoria por Área
            if (areaId.HasValue && areaId != Guid.Empty)
            {
                entities = entities.Where(e => e.AreaId == areaId.Value).ToList();
            }

            // 3. Mapeo y Verificación de Dependencias (AQUÍ ESTÁ EL CAMBIO)
            var dtos = new List<TipoSolicitudDto>();

            foreach (var entity in entities)
            {
                // Usamos tu helper existente
                var dto = MapToDto(entity);

                // ✅ CONSULTAMOS SI TIENE TICKETS
                // Esto es lo que permite al Frontend saber si oculta el botón "Eliminar"
                dto.TieneTickets = await _ticketRepository.AnyAsync(t => t.TipoSolicitudId == entity.Id, cancellationToken);

                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<TipoSolicitudDto?> GetByIdAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(tipoSolicitudId).ConfigureAwait(false);
            if (entity == null) return null;

            return MapToDto(entity);
        }

        public async Task<TipoSolicitudDto> CreateAsync(TipoSolicitudDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = new TipoSolicitud
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Nombre = dto.Nombre,
                AreaId = dto.AreaId,
                Activo = dto.Activo
            };

            await _repository.AddAsync(entity).ConfigureAwait(false);
            await RegistrarEvento(SystemEventType.TipoSolicitudCreado, entity.Nombre);
            return MapToDto(entity);
        }

        public async Task<TipoSolicitudDto> UpdateAsync(Guid tipoSolicitudId, TipoSolicitudDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _repository.GetByIdAsync(tipoSolicitudId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"TipoSolicitud con id '{tipoSolicitudId}' no encontrada.");

            existing.Nombre = dto.Nombre;
            existing.AreaId = dto.AreaId;
            existing.Activo = dto.Activo;

            await _repository.UpdateAsync(existing).ConfigureAwait(false);
            await RegistrarEvento(SystemEventType.TipoSolicitudEstadoCambio, existing.Nombre);
            return MapToDto(existing);
        }

        // ============================================================
        // ACTIONS (Activar / Desactivar / Borrar)
        // ============================================================

        public async Task ActivateAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(tipoSolicitudId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"TipoSolicitud con id '{tipoSolicitudId}' no encontrada.");

            existing.Activo = true;
            await RegistrarEvento(SystemEventType.TipoSolicitudEstadoCambio, $"{existing.Nombre} (Activado)");
            await _repository.UpdateAsync(existing).ConfigureAwait(false);
        }

        public async Task DeactivateAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(tipoSolicitudId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"TipoSolicitud con id '{tipoSolicitudId}' no encontrada.");

            existing.Activo = false;
            await RegistrarEvento(SystemEventType.TipoSolicitudEstadoCambio, $"{existing.Nombre} (Desactivado)");
            await _repository.UpdateAsync(existing).ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default)
        {

            bool tieneTicketsAbiertos = await _ticketRepository.AnyAsync(t =>
        t.TipoSolicitudId == tipoSolicitudId &&
        t.Estado != EstadoTicket.Cerrada &&
        t.Estado != EstadoTicket.Rechazada, cancellationToken);

            if (tieneTicketsAbiertos)
            {
                throw new InvalidOperationException("No se puede eliminar el tipo: existen tickets pendientes.");
            }

            // 1. Buscamos el registro
            var existing = await _repository.GetByIdAsync(tipoSolicitudId).ConfigureAwait(false);

            if (existing == null)
                throw new KeyNotFoundException($"TipoSolicitud con id '{tipoSolicitudId}' no encontrada.");

            // 2. VERIFICACIÓN: ¿Está siendo usado en algún ticket?
            // (Asumo que tu repositorio de tickets tiene un método AnyAsync o similar. 
            //  Si no lo tienes, debes agregarlo en ITicketRepository).
            bool estaEnUso = await _ticketRepository.AnyAsync(t => t.TipoSolicitudId == tipoSolicitudId);

            if (estaEnUso)
            {
                // === CAMINO A: SOFT DELETE (Historial Protegido) ===
                // Como tiene tickets, NO borramos. Solo desactivamos.
                existing.Activo = false;
                await _repository.UpdateAsync(existing).ConfigureAwait(false);
            }
            else
            {
                // === CAMINO B: HARD DELETE (Limpieza Real) ===
                // Nadie lo usa. Lo borramos físicamente de la base de datos.
                await _repository.DeleteAsync(existing).ConfigureAwait(false);
            }

            await RegistrarEvento(SystemEventType.TipoSolicitudEliminado, existing.Nombre);
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private async Task RegistrarEvento(SystemEventType tipo, string detalles)
{
    var user = _httpContextAccessor.HttpContext?.User;

    // Buscamos el ID en 'uid', 'NameIdentifier' o 'sub'
    var userIdStr = user?.FindFirst("uid")?.Value 
                 ?? user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                 ?? user?.FindFirst("sub")?.Value;

    if (Guid.TryParse(userIdStr, out Guid userId))
    {
        await _eventRepository.AddAsync(new SystemEvent
        {
            UsuarioId = userId,
            Tipo = tipo,
            Detalles = detalles,
            Fecha = DateTime.UtcNow
        });
    }
}
        private static TipoSolicitudDto MapToDto(TipoSolicitud entity)
            => new TipoSolicitudDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                AreaId = entity.AreaId,
                Activo = entity.Activo
            };


    }
}