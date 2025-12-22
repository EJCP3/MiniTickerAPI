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
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _areaRepository;

        public AreaService(IAreaRepository areaRepository)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
        }

        public async Task<IReadOnlyList<AreaDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var areas = await _areaRepository.GetAllAsync().ConfigureAwait(false);

            return areas.Select(MapToDto).ToList();
        }

        public async Task<AreaDto?> GetByIdAsync(Guid areaId, CancellationToken cancellationToken = default)
        {
            var area = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (area == null) return null;

            return MapToDto(area);
        }

        public async Task<AreaDto> CreateAsync(AreaDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = new Area
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Nombre = dto.Nombre,
                Activo = dto.Activo
            };

            await _areaRepository.AddAsync(entity).ConfigureAwait(false);

            return MapToDto(entity);
        }

        public async Task<AreaDto> UpdateAsync(Guid areaId, AreaDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"Área con id '{areaId}' no encontrada.");

            // Actualizar campos permitidos
            existing.Nombre = dto.Nombre;
            existing.Activo = dto.Activo;

            await _areaRepository.UpdateAsync(existing).ConfigureAwait(false);

            return MapToDto(existing);
        }

        public async Task ActivateAsync(Guid areaId, CancellationToken cancellationToken = default)
        {
            var existing = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"Área con id '{areaId}' no encontrada.");

            existing.Activo = true;
            await _areaRepository.UpdateAsync(existing).ConfigureAwait(false);
        }

        public async Task DeactivateAsync(Guid areaId, CancellationToken cancellationToken = default)
        {
            var existing = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"Área con id '{areaId}' no encontrada.");

            existing.Activo = false;
            await _areaRepository.UpdateAsync(existing).ConfigureAwait(false);
        }

        #region Helpers

        private static AreaDto MapToDto(Area entity)
            => new AreaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Activo = entity.Activo
            };

        #endregion
    }
}
