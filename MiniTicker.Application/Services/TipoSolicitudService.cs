using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MiniTicker.Core.Application.Catalogs;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Core.Application.Services
{
    public class TipoSolicitudService : ITipoSolicitudService
    {
        private readonly ITipoSolicitudRepository _repository;

        public TipoSolicitudService(ITipoSolicitudRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // ============================================================
        // GET ALL (Fusionado: Maneja Filtro por Área y Filtro de Inactivos)
        // ============================================================
        public async Task<IReadOnlyList<TipoSolicitudDto>> GetAllAsync(
            Guid? areaId = null,
            bool incluirInactivos = false,
            CancellationToken cancellationToken = default)
        {
            // 1. Obtenemos la lista base desde el repositorio (aplicando el filtro de activo/inactivo)
            var entities = await _repository.GetAllAsync(incluirInactivos);

            // 2. Si nos pidieron filtrar por Área específica, aplicamos el filtro en memoria (LINQ)
            if (areaId.HasValue && areaId != Guid.Empty)
            {
                // Filtramos sobre la lista que ya trajimos
                var filtradas = entities.Where(e => e.AreaId == areaId.Value).ToList();
                return filtradas.Select(MapToDto).ToList();
            }

            // 3. Si no hay filtro de área, devolvemos todo lo que trajo el repo
            return entities.Select(MapToDto).ToList();
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
            await _repository.UpdateAsync(existing).ConfigureAwait(false);
        }

        public async Task DeactivateAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(tipoSolicitudId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"TipoSolicitud con id '{tipoSolicitudId}' no encontrada.");

            existing.Activo = false;
            await _repository.UpdateAsync(existing).ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default)
        {
            // 1. Buscamos el registro
            var existing = await _repository.GetByIdAsync(tipoSolicitudId).ConfigureAwait(false);

            if (existing == null)
                throw new KeyNotFoundException($"TipoSolicitud con id '{tipoSolicitudId}' no encontrada.");

            // 2. BORRADO LÓGICO (Soft Delete)
            existing.Activo = false;

            // 3. Actualizamos
            await _repository.UpdateAsync(existing).ConfigureAwait(false);
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private static TipoSolicitudDto MapToDto(TipoSolicitud entity)
            => new TipoSolicitudDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                AreaId = entity.AreaId,
                Activo = entity.Activo
            };

        // NOTA: He borrado todo el bloque de "Implementación explícita" que tenías aquí abajo.
        // Ya no es necesario porque los métodos públicos de arriba cumplen con la interfaz.
    }
}