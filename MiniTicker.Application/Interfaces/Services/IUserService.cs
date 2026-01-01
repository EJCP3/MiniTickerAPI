using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MiniTicker.Core.Application.Users;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IUserService
    {
     
        Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

   
        Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

       
        Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default);
        Task<UserDto> UpdateAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default);

       
        Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default);
        Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}