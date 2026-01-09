using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniTicker.Core.Application.Users;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Core.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task<Usuario?> GetByIdAsync(Guid id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default);
        Task DeleteAsync(Usuario usuario);

        

    }
}