using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Users;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;

        public UserService(IUserRepository userRepository, IFileStorageService fileStorageService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        }

        public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var usuario = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
            if (usuario == null) return null;

            return MapToDto(usuario);
        }

        public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync().ConfigureAwait(false);
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var usuario = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
            if (usuario == null) throw new KeyNotFoundException($"Usuario con id '{userId}' no encontrado.");

            usuario.Nombre = dto.Nombre ?? usuario.Nombre;

            if (dto.FotoPerfil != null)
            {
                var fotoUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios").ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(fotoUrl))
                {
                    usuario.FotoPerfilUrl = fotoUrl;
                }
            }

            await _userRepository.UpdateAsync(usuario).ConfigureAwait(false);

            return MapToDto(usuario);
        }

        public async Task<UserDto> ChangeRoleAsync(Guid userId, Rol newRole, CancellationToken cancellationToken = default)
        {
            var usuario = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
            if (usuario == null) throw new KeyNotFoundException($"Usuario con id '{userId}' no encontrado.");

            usuario.Rol = newRole;
            await _userRepository.UpdateAsync(usuario).ConfigureAwait(false);

            return MapToDto(usuario);
        }

        public async Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var usuario = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
            if (usuario == null) throw new KeyNotFoundException($"Usuario con id '{userId}' no encontrado.");

            usuario.Activo = true;
            await _userRepository.UpdateAsync(usuario).ConfigureAwait(false);
        }

        public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var usuario = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
            if (usuario == null) throw new KeyNotFoundException($"Usuario con id '{userId}' no encontrado.");

            usuario.Activo = false;
            await _userRepository.UpdateAsync(usuario).ConfigureAwait(false);
        }

        // Implementación faltante
        public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("El email es obligatorio.", nameof(dto.Email));
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(dto.Nombre));

            // Verificar duplicados por email
            var existente = await _userRepository.GetByEmailAsync(dto.Email).ConfigureAwait(false);
            if (existente != null)
                throw new InvalidOperationException($"Ya existe un usuario con el email '{dto.Email}'.");

            // Subida opcional de foto
            string? fotoUrl = null;
            if (dto.FotoPerfil != null)
            {
                fotoUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios").ConfigureAwait(false);
            }

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Email = dto.Email,
                Rol = dto.Rol,
                AreaId = dto.AreaId,
                Activo = dto.Activo,
                FotoPerfilUrl = fotoUrl,
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = null
            };

            await _userRepository.AddAsync(usuario).ConfigureAwait(false);

            return MapToDto(usuario);
        }

        #region Helpers

        private static UserDto MapToDto(Usuario u)
            => new UserDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol,
                FotoPerfilUrl = u.FotoPerfilUrl
            };

        #endregion  
    }
}