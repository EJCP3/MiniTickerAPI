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

            // Actualizar nombre
            usuario.Nombre = dto.Nombre ?? usuario.Nombre;

            // Si viene foto, almacenarla usando IFileStorageService y actualizar la URL
            if (dto.FotoPerfil != null)
            {
                // Se asume que IFileStorageService expone un método que recibe IFormFile y devuelve la URL (string).
                // Nombre del método: UploadAsync (string carpeta, IFormFile file, CancellationToken token) o similar.
                // Aquí se utiliza una convención habitual; ajustar si la implementación real difiere.
                var fotoUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios", cancellationToken).ConfigureAwait(false);
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

    // Nota: Interfaz mínima esperada para el servicio de almacenamiento de archivos.
    // Si la implementación real tiene otro nombre de método u orden de parámetros,
    // adapta la llamada en UpdateProfileAsync en consecuencia.
    public interface IFileStorageService
    {
        /// <summary>
        /// Sube el fichero y devuelve la URL pública o ruta relativa.
        /// </summary>
        Task<string?> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    }
}