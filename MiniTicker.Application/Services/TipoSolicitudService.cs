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
    internal class TipoSolicitudService : ITipoSolicitudService
    {
        private readonly ITipoSolicitudRepository _repository;

        public TipoSolicitudService(ITipoSolicitudRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IReadOnlyList<TipoSolicitudDto>> GetAllAsync(Guid? areaId = null, CancellationToken cancellationToken = default)
        {
            // Si se proporciona areaId, obtener por área; si no, solicitar todos pasándole Guid.Empty.
            // Nota: el repositorio debe soportar Guid.Empty para devolver todos, o bien implementar GetAllAsync en el repositorio.
            var entities = areaId.HasValue
                ? await _repository.GetByAreaIdAsync(areaId.Value).ConfigureAwait(false)
                : await _repository.GetByAreaIdAsync(Guid.Empty).ConfigureAwait(false);

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
                AreaId = dto.Id == Guid.Empty ? dto.Id : dto.Id, // map dto.AreaId if present in DTO; DTO currently has AreaId property
                Activo = dto.Activo
            };

            // If DTO has AreaId property, use it (DTO defined as TipoSolicitudDto includes AreaId)
            // Fix: assign correctly
            entity.AreaId = dto.AreaId;

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

        #region Helpers

        private static TipoSolicitudDto MapToDto(TipoSolicitud entity)
            => new TipoSolicitudDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                AreaId = entity.AreaId,
                Activo = entity.Activo
            };

        #endregion
        public async Task<IReadOnlyList<TipoSolicitudDto>> GetByAreaIdAsync(
    Guid areaId,
    CancellationToken cancellationToken = default)
        {
            var entities = await _repository
                .GetByAreaIdAsync(areaId)
                .ConfigureAwait(false);

            return entities.Select(MapToDto).ToList();
        }

        public async Task DeleteAsync(
            Guid tipoSolicitudId,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository
                .GetByIdAsync(tipoSolicitudId)
                .ConfigureAwait(false);

            if (existing == null)
                throw new KeyNotFoundException(
                    $"TipoSolicitud con id '{tipoSolicitudId}' no encontrada.");

            await _repository.DeleteAsync(existing)
                .ConfigureAwait(false);
        }

        // Implementación explícita para cumplir con la interfaz ITipoSolicitudService
        async Task ITipoSolicitudService.GetByAreaIdAsync(Guid areaId)
        {
            await GetByAreaIdAsync(areaId);
        }

        async Task ITipoSolicitudService.DeleteAsync(Guid id)
        {
            await DeleteAsync(id);
        }
    }



}