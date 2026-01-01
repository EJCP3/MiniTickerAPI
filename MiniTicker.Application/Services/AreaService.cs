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

        // ✅ CORREGIDO: Usa CreateAreaDto y _areaRepository
        public async Task<AreaDto> CreateAsync(CreateAreaDto dto, CancellationToken cancellationToken)
        {
            string prefijoGenerado = GeneratePrefix(dto.Nombre);

            var entity = new Area
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Prefijo = prefijoGenerado,
                Activo = true
            };

            // ✅ CORREGIDO: Usar _areaRepository en lugar de _repository
            await _areaRepository.AddAsync(entity);

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
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _areaRepository.GetByIdAsync(areaId).ConfigureAwait(false);
            if (existing == null) throw new KeyNotFoundException($"Área con id '{areaId}' no encontrada.");

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

        public async Task DeleteAsync(Guid areaId, CancellationToken cancellationToken = default)
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
                // ✅ Agregamos Prefijo al DTO para que se vea en el Frontend
                Prefijo = entity.Prefijo,
                Activo = entity.Activo
            };
        #endregion
    }
}