using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MiniTicker.Core.Application.Users;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para la gestión de usuarios.
    /// Contrato sin implementación; pensado para uso por roles Admin / SuperAdmin.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtiene un usuario por su identificador.
        /// Devuelve null si no existe.
        /// </summary>
        Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los usuarios del sistema.
        /// </summary>
        Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza el perfil de un usuario (nombre, foto, etc.) y devuelve el DTO actualizado.
        /// </summary>
        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cambia el rol de un usuario y devuelve el DTO actualizado.
        /// </summary>
        Task<UserDto> ChangeRoleAsync(Guid userId, Rol newRole, CancellationToken cancellationToken = default);

        /// <summary>
        /// Activa un usuario.
        /// </summary>
        Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Desactiva un usuario.
        /// </summary>
        Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}