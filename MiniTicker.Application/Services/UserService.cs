using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public UserService(
            IUserRepository userRepository,
            IFileStorageService fileStorageService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        }

        // ============================================================
        // GET ALL
        // ============================================================
        public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // Asumiendo que tu repositorio tiene GetAllAsync. Si no, avísame.
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(MapToUserDto).ToList();
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user == null ? null : MapToUserDto(user);
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var user = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Email = dto.Email,
                Rol = dto.Rol,
                Activo = dto.Activo,
                // Hasheamos la contraseña
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FechaCreacion = DateTime.UtcNow
            };

            // Manejo de foto si existe
            if (dto.FotoPerfil != null)
            {
                user.FotoPerfilUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios");
            }

            await _userRepository.AddAsync(user);
            return MapToUserDto(user);
        }

        // ============================================================
        // UPDATE (Perfil y Completo)
        // ============================================================
        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");

            user.Nombre = dto.Nombre;
            if (dto.FotoPerfil != null)
            {
                user.FotoPerfilUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios");
            }

            await _userRepository.UpdateAsync(user);
            return MapToUserDto(user);
        }

        public async Task<UserDto> UpdateAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");

            user.Nombre = dto.Nombre;
            user.Email = dto.Email;
            user.Rol = dto.Rol;
            user.AreaId = dto.AreaId;
            user.Activo = dto.Activo;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.FotoPerfil != null)
            {
                user.FotoPerfilUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios");
            }

            await _userRepository.UpdateAsync(user);
            return MapToUserDto(user);
        }

        // ============================================================
        // ACTIVATE / DEACTIVATE
        // ============================================================
        public async Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

            user.Activo = true;
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

            user.Activo = false;
            await _userRepository.UpdateAsync(user);
        }
        public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // 1. Buscamos el usuario
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

            // 2. Intentamos borrarlo físicamente
            // NOTA: Esto lanzará una excepción si el usuario tiene Tickets asociados.
            await _userRepository.DeleteAsync(user);
        }
        // ============================================================
        // HELPER
        // ============================================================
        private static UserDto MapToUserDto(Usuario user)
        {
            return new UserDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                Rol = user.Rol,
                FotoPerfilUrl = user.FotoPerfilUrl
                // Agrega AreaId si tu UserDto lo tiene
            };
        }
    }
}