using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            await _context.Usuarios.AddAsync(usuario).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Usuario?> GetByIdAsync(Guid id)
        {
          return await _context.Usuarios
        .Include(u => u.Area) // 👈 Carga el objeto Area relacionado
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == id)
        .ConfigureAwait(false);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio.", nameof(email));

           return await _context.Usuarios
        .Include(u => u.Area) // 👈 Carga el objeto Area relacionado
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Email == email)
        .ConfigureAwait(false);
        }
        public async Task DeleteAsync(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            // Esto borra el registro físicamente de la tabla Usuarios
            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();
        }
        public async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken)
        {
            var list = await _context.Usuarios
        .Include(u => u.Area) // 👈 Carga el objeto Area para toda la lista
        .AsNoTracking()
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);

            return list;
        }
 
        
    }
}
