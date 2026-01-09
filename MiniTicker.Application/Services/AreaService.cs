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
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _areaRepository;

        private readonly ITicketRepository _ticketRepository;

        private readonly IUserRepository _userRepository; 

        private readonly ISystemEventRepository _eventRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AreaService(IAreaRepository areaRepository, ITicketRepository ticketRepository, IUserRepository userRepository, ISystemEventRepository eventRepository, IHttpContextAccessor httpContextAccessor)
        {

            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private async Task ValidarResponsableUnico(Guid? responsableId, Guid? areaIdActual = null)
        {
            if (!responsableId.HasValue) return;

            // Buscamos si existe OTRA área que ya tenga a este mismo responsable
            var areas = await _areaRepository.GetAllAsync();
            var areaConResponsable = areas.FirstOrDefault(a =>
                a.ResponsableId == responsableId &&
                a.Id != areaIdActual); // Ignoramos el área actual si estamos editando

            if (areaConResponsable != null)
            {
                throw new InvalidOperationException($"Este usuario ya es responsable del área '{areaConResponsable.Nombre}'. Un gestor solo puede estar vinculado a un área a la vez.");
            }
        }
        public async Task<IReadOnlyList<AreaDto>> GetAllAsync(bool incluirInactivos = false, CancellationToken cancellationToken = default)
        {
            var entities = await _areaRepository.GetAllAsync(incluirInactivos, cancellationToken);
            return entities.Select(MapToDto).ToList();
        }

        public async Task<AreaDto?> GetByIdAsync(Guid areaId, CancellationToken cancellationToken = default)
        {
            var area = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (area == null) return null;
            return MapToDto(area);
        }

        public async Task<AreaDto> CreateAsync(CreateAreaDto dto, CancellationToken cancellationToken)
        {
            await ValidarResponsableUnico(dto.ResponsableId);
            string prefijoGenerado = GeneratePrefix(dto.Nombre);

            var entity = new Area
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Prefijo = prefijoGenerado,
                Activo = true,
                // ✅ NUEVO: Guardamos el Responsable seleccionado en el Modal
                ResponsableId = dto.ResponsableId
            };

            await _areaRepository.AddAsync(entity);

            if (entity.ResponsableId.HasValue)
            {
                var usuario = await _userRepository.GetByIdAsync(entity.ResponsableId.Value);
                if (usuario != null)
                {
                    usuario.AreaId = entity.Id;
                    await _userRepository.UpdateAsync(usuario);
                }
            }

            await RegistrarEvento(SystemEventType.AreaCreada, entity.Nombre);
            return MapToDto(entity);
        }

        private string GeneratePrefix(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "GEN";
            var limpio = nombre.Trim().ToUpper();
            return limpio.Length >= 3 ? limpio.Substring(0, 3) : limpio;
        }

        public async Task<AreaDto> UpdateAsync(Guid areaId, AreaDto dto, CancellationToken cancellationToken = default)
        {
            // 1. Validar responsable único (ignorando esta área misma)
            await ValidarResponsableUnico(dto.ResponsableId, areaId);

            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"Área con id '{areaId}' no encontrada.");

            // Guardamos el responsable anterior para saber si cambió
            var responsableAnteriorId = existing.ResponsableId;

            existing.Nombre = dto.Nombre;
            existing.Activo = dto.Activo;
            existing.Prefijo = dto.Prefijo;
            existing.ResponsableId = dto.ResponsableId;

            await _areaRepository.UpdateAsync(existing).ConfigureAwait(false);

            // ✅ NUEVO: Sincronizar el nuevo usuario responsable
            if (existing.ResponsableId.HasValue && existing.ResponsableId != responsableAnteriorId)
            {
                var usuario = await _userRepository.GetByIdAsync(existing.ResponsableId.Value);
                if (usuario != null)
                {
                    usuario.AreaId = existing.Id;
                    await _userRepository.UpdateAsync(usuario);
                }
            }
            await RegistrarEvento(SystemEventType.AreaActualizada, existing.Nombre);
            return MapToDto(existing);
        }

        public async Task ActivateAsync(Guid areaId, CancellationToken cancellationToken = default)
        {
            var existing = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"Área con id '{areaId}' no encontrada.");
            existing.Activo = true;

            await RegistrarEvento(SystemEventType.AreaEstadoCambio, $"{existing.Nombre} (Activado)");
            await _areaRepository.UpdateAsync(existing).ConfigureAwait(false);
        }

        public async Task DeactivateAsync(Guid areaId, CancellationToken cancellationToken = default)
        {
            await ValidarTicketsPendientes(areaId); // 👈 Validación antes de desactivar

            var existing = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"Área con id '{areaId}' no encontrada.");

            existing.Activo = false;
            await RegistrarEvento(SystemEventType.AreaEstadoCambio, $"{existing.Nombre} (Desactivado)");
            await _areaRepository.UpdateAsync(existing).ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid areaId, CancellationToken cancellationToken = default)
        {
            await ValidarTicketsPendientes(areaId);

            var area = await _areaRepository.GetByIdAsync(areaId);
            if (area == null) throw new KeyNotFoundException("Área no encontrada");

            try
            {
                // 2. Intentamos el borrado físico
                await _areaRepository.DeleteAsync(area);
            }
            catch (Exception)
            {
                // 3. Si falla por integridad referencial (tiene tickets históricos), 
                // lanzamos un mensaje amigable en lugar del error técnico
                throw new InvalidOperationException("No se puede eliminar físicamente: esta área tiene historial de tickets. Se recomienda desactivarla en su lugar.");
            }
            await RegistrarEvento(SystemEventType.AreaEliminada, area.Nombre);
        }
        public async Task QuitarResponsableArea(Guid areaId, Guid usuarioId)
        {
            // 1. Recuperamos las entidades. 
            var area = await _areaRepository.GetByIdAsync(areaId);
            var usuario = await _userRepository.GetByIdAsync(usuarioId);

            if (area == null || usuario == null) return;

            // 2. Limpiamos las propiedades de navegación y las claves foráneas
            // Esto rompe el vínculo en ambos lados de la relación configurada en Fluent API
            area.ResponsableId = null;
            area.Responsable = null;

            usuario.AreaId = null;
            usuario.Area = null;

            // 3. Usamos una técnica de "Desvinculación" manual si el error persiste
            // Al llamar a UpdateAsync en el repositorio, asegúrate de que guarde cambios.
            // Si el error 400 continúa, es necesario que el repositorio use .AsNoTracking() al buscar.
            
            await RegistrarEvento(SystemEventType.AreaResponsableQuitar, $"Área: {area.Nombre}, Usuario: {usuario.Nombre}");


            await _areaRepository.UpdateAsync(area);
            await _userRepository.UpdateAsync(usuario);
          
        }
        // =====================================================
        // MÉTODO DE VALIDACIÓN (REGLA DE NEGOCIO)
        // =====================================================
        private async Task ValidarTicketsPendientes(Guid areaId)
        {
            // Buscamos si existe ALGÚN ticket que NO esté en los estados finales
            // Debes tener acceso a un método en tu repositorio que permita filtrar por área y estado
            var tickets = await _ticketRepository.GetAllAsync(); // Asumiendo que trae la lista completa

            var tienePendientes = tickets.Any(t =>
                t.AreaId == areaId &&
                t.Estado != EstadoTicket.Cerrada &&
                t.Estado != EstadoTicket.Rechazada);

            if (tienePendientes)
            {
                // Esta excepción será capturada por tu Middleware o Controller
                throw new InvalidOperationException("No se puede desactivar/eliminar el área. Existen tickets en proceso o nuevos.");
            }
        }
        #region Helpers
        private static AreaDto MapToDto(Area entity)
        {
            var listaTickets = entity.Tickets ?? new List<Ticket>();

            // Calculamos primero las completadas para reutilizar lógica si quieres
            // OJO: Asegúrate de tener 'Rechazada' en tu Enum EstadoTicket
            var conteoCompletadas = listaTickets.Count(t =>
                t.Estado == EstadoTicket.Cerrada ||
                t.Estado == EstadoTicket.Rechazada);

            return new AreaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Prefijo = entity.Prefijo,
                Activo = entity.Activo,
                ResponsableId = entity.ResponsableId,

                Stats = new AreaStatsDto
                {
                    Total = listaTickets.Count,

                    // Completadas: Sumamos Cerradas + Rechazadas
                    Completadas = conteoCompletadas,

                    // Activas: Son todas las que NO están ni cerradas ni rechazadas
                    // Opción A (Matemática simple y rápida):
                    Activas = listaTickets.Count - conteoCompletadas

                    // Opción B (Explícita, por si prefieres leerlo):
                    // Activas = listaTickets.Count(t => 
                    //    t.Estado != EstadoTicket.Cerrado && 
                    //    t.Estado != EstadoTicket.Rechazada)
                }
            };
        }

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
        #endregion
    }
}