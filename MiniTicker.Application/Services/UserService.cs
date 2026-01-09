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
using Microsoft.AspNetCore.Http; // Necesario para IHttpContextAccessor

namespace MiniTicker.Core.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly IAreaRepository _areaRepository; // <--- NUEVO
        private readonly IFileStorageService _fileStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor; // Inyección de IHttpContextAccessor
        private readonly ISystemEventRepository _systemEventRepository; // Inyección de ISystemEventRepository

        public UserService(
            IUserRepository userRepository,
            IAreaRepository areaRepository,
            IFileStorageService fileStorageService,
            IHttpContextAccessor httpContextAccessor, ISystemEventRepository systemEventRepository) // Inyección en el constructor
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository)); // <--- NUEVO
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _systemEventRepository = systemEventRepository ?? throw new ArgumentNullException(nameof(systemEventRepository));
        }



        private async Task ValidarAreaDisponible(Guid? areaId, Guid? usuarioIdActual = null)
        {
            if (!areaId.HasValue) return;

            // 1. Buscamos el área que se quiere asignar
            var area = await _areaRepository.GetByIdAsync(areaId.Value);

            // 2. Si el área ya tiene un responsable y no es el mismo usuario que estamos editando
            if (area != null && area.ResponsableId.HasValue && area.ResponsableId != usuarioIdActual)
            {
                // 3. Buscamos el nombre del responsable actual para dar un mensaje más claro
                var responsableActual = await _userRepository.GetByIdAsync(area.ResponsableId.Value);
                string nombreResponsable = responsableActual?.Nombre ?? "otro usuario";

                throw new InvalidOperationException($"El área '{area.Nombre}' ya tiene a {nombreResponsable} como responsable. Desvincúlelo primero o elija otra área.");
            }
        }
        public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
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
            await ValidarAreaDisponible(dto.AreaId);

            var user = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Email = dto.Email,
                Rol = dto.Rol,
                Activo = dto.Activo,
                AreaId = dto.AreaId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FechaCreacion = DateTime.UtcNow
            };

            if (dto.FotoPerfil != null)
            {
                user.FotoPerfilUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios");
            }

            await _userRepository.AddAsync(user);

            // ✅ SINCRONIZACIÓN: El área ahora tiene este nuevo responsable
            if (user.AreaId.HasValue)
            {
                var area = await _areaRepository.GetByIdAsync(user.AreaId.Value);
                if (area != null)
                {
                    area.ResponsableId = user.Id;
                    await _areaRepository.UpdateAsync(area);
                }
            }

            await RegistrarEvento(null, SystemEventType.UsuarioCreado, user.Nombre);

            return MapToUserDto(user);
        }

        // ============================================================
        // UPDATE (Perfil y Completo)
        // ============================================================
        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");

            if (dto.AreaId.HasValue && dto.AreaId != user.AreaId)
            {
                await ValidarAreaDisponible(dto.AreaId, userId);
            }
            user.Nombre = dto.Nombre;
            // No actualizamos Email ni Rol en UpdateProfileAsync típicamente, pero depende de tus requerimientos.
            // Si quieres permitir cambiar email aquí: user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.FotoPerfil != null)
            {
                if (!string.IsNullOrEmpty(user.FotoPerfilUrl))
                {
                    await _fileStorageService.DeleteAsync(user.FotoPerfilUrl);
                }
                user.FotoPerfilUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios");
            }

            await _userRepository.UpdateAsync(user);
            await RegistrarEvento(null, SystemEventType.UsuarioActualizado, user.Nombre);
            return MapToUserDto(user);
        }

        public async Task<UserDto> UpdateAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");

            // Guardamos el área anterior para limpiar vínculos si cambia
            var areaAnteriorId = user.AreaId;

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
                if (!string.IsNullOrEmpty(user.FotoPerfilUrl))
                {
                    await _fileStorageService.DeleteAsync(user.FotoPerfilUrl);
                }
                user.FotoPerfilUrl = await _fileStorageService.UploadAsync(dto.FotoPerfil, "usuarios");
            }

            await _userRepository.UpdateAsync(user);

            // ✅ CASO A: Se asignó una nueva área (o cambió)
            if (user.AreaId.HasValue && user.AreaId != areaAnteriorId)
            {
                var nuevaArea = await _areaRepository.GetByIdAsync(user.AreaId.Value);
                if (nuevaArea != null)
                {
                    nuevaArea.ResponsableId = user.Id;
                    await _areaRepository.UpdateAsync(nuevaArea);
                }
            }

            // ✅ CASO B: El usuario dejó el área anterior o se cambió a una nueva
            // Debemos limpiar el ResponsableId del área que acaba de dejar
            if (areaAnteriorId.HasValue && user.AreaId != areaAnteriorId)
            {
                var areaVieja = await _areaRepository.GetByIdAsync(areaAnteriorId.Value);
                if (areaVieja != null && areaVieja.ResponsableId == userId)
                {
                    areaVieja.ResponsableId = null;
                    await _areaRepository.UpdateAsync(areaVieja);
                }
            }

            if (user.Rol == Rol.Solicitante && user.AreaId != null)
            {
                // Limpiamos el vínculo en el área antes de quitarle el ID al usuario
                var area = await _areaRepository.GetByIdAsync(user.AreaId.Value);
                if (area != null)
                {
                    area.ResponsableId = null;
                    await _areaRepository.UpdateAsync(area);
                }
                user.AreaId = null;
            }
            await RegistrarEvento(null, SystemEventType.UsuarioActualizado, user.Nombre);
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
            await RegistrarEvento(null, SystemEventType.UsuarioEstadoCambio, $"{user.Nombre} (Activado)");
        }

        public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

            user.Activo = false;
            await _userRepository.UpdateAsync(user);
            await RegistrarEvento(null, SystemEventType.UsuarioEstadoCambio, $"{user.Nombre} (Desactivado)");
        }

        public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

            await _userRepository.DeleteAsync(user);
        }

        public async Task<IEnumerable<UserDto>> GetActiveManagersAsync(CancellationToken cancellationToken = default)
        {
            // 1. Obtenemos las entidades 'Usuario' desde el repositorio que acabas de mostrar
            var users = await _userRepository.GetAllAsync(cancellationToken);

            // 2. Filtramos los usuarios que están activos y tienen roles administrativos
            var activeManagers = users.Where(u =>
                u.Activo &&
                (u.Rol == Rol.Gestor)
            );

            // 3. Mapeamos a DTO usando tu función helper
            // Esto soluciona el error CS0411 al ser explícitos: u => MapToUserDto(u)
            return activeManagers.Select(u => MapToUserDto(u)).ToList();
        }

        // ============================================================
        // HELPER
        // ============================================================
        private UserDto MapToUserDto(Usuario user)
        {
            string fotoUrlCompleta = null;

            if (!string.IsNullOrEmpty(user.FotoPerfilUrl))
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    // Combina: http://localhost:5232 + / + uploads/usuarios/foto.jpg
                    fotoUrlCompleta = $"{baseUrl}/{user.FotoPerfilUrl.TrimStart('/')}";
                }
                else
                {
                    // Fallback si no hay contexto HTTP (ej. background jobs)
                    fotoUrlCompleta = user.FotoPerfilUrl;
                }
            }

            return new UserDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                Rol = user.Rol.ToString(),
                FotoPerfilUrl = fotoUrlCompleta,
                FechaCreacion = user.FechaCreacion,
                Activo = user.Activo,
                AreaId = user.AreaId,
                // Si tu Usuario tiene la relación virtual con Area, puedes mandar el nombre también:
                AreaNombre = user.Area?.Nombre
                // AreaId = user.AreaId // Descomentar si UserDto y Usuario tienen esta propiedad
            };
        }

        private async Task RegistrarEvento(Guid? usuarioId, SystemEventType tipo, string detalles)
        {
            // Si no viene un ID explícito (ej. al crear usuario, pasamos null), buscamos al "Actor" (quien está logueado)
            if (!usuarioId.HasValue)
            {
                var user = _httpContextAccessor.HttpContext?.User;

                // 1. Intentamos obtener el ID de los claims estándar
                var userIdStr = user?.FindFirst("uid")?.Value
                             ?? user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                             ?? user?.FindFirst("sub")?.Value;

                if (Guid.TryParse(userIdStr, out Guid id))
                {
                    usuarioId = id;
                }
            }

            // Si logramos obtener un ID (ya sea pasado por parámetro o del token), guardamos
            if (usuarioId.HasValue)
            {
                await _systemEventRepository.AddAsync(new SystemEvent
                {
                    UsuarioId = usuarioId.Value,
                    Tipo = tipo,
                    Detalles = detalles,
                    Fecha = DateTime.UtcNow
                });
            }
        }
    }
}