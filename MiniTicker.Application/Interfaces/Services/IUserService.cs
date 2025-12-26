using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MiniTicker.Core.Application.Users;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto, CancellationToken cancellationToken = default);
        Task<UserDto> ChangeRoleAsync(Guid userId, Rol newRole, CancellationToken cancellationToken = default);
        Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default);
        Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    }
}