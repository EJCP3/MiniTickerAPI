using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniTicker.Core.Application.Auth;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Application.Users;

namespace MiniTicker.Core.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemEventRepository _eventRepository;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, ISystemEventRepository eventRepository)
        {
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher
                ?? throw new ArgumentNullException(nameof(passwordHasher));
            _configuration = configuration
                ?? throw new ArgumentNullException(nameof(configuration));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _httpContextAccessor = httpContextAccessor;
        }

        // =====================================================
        // LOGIN
        // =====================================================
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var usuario = await _userRepository.GetByEmailAsync(dto.Email);

            // 2. Validar solo si existe (Seguridad: no revelar si el email es válido o no)

            // 2. Si no existe, error genérico (por seguridad)
            if (usuario == null)
            {
                throw new UnauthorizedAccessException("CREDENTIALS_INVALID");
            }

            // 3. Si existe pero está desactivado, mensaje específico
            if (!usuario.Activo)
            {
                // Lanzamos una excepción que podamos identificar en el Controller
                throw new InvalidOperationException("USER_LOCKED");
            }

            // 4. Si está activo, verificar la contraseña
            if (!_passwordHasher.Verify(dto.Password, usuario.PasswordHash))
            {
                throw new UnauthorizedAccessException("CREDENTIALS_INVALID");
            }


            string fotoUrlCompleta = null;
            if (!string.IsNullOrEmpty(usuario.FotoPerfilUrl))
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    fotoUrlCompleta = $"{baseUrl}/{usuario.FotoPerfilUrl.TrimStart('/')}";
                }
            }

            // Registrar evento de login
            await _eventRepository.AddAsync(new SystemEvent
            {
                Tipo = Core.Domain.Enums.SystemEventType.Login,
                Detalles = $"Usuario {usuario.Email} inició sesión.",
                Fecha = DateTime.UtcNow,
                UsuarioId = usuario.Id
            });

            var token = GenerateJwtToken(usuario);

            return new LoginResponseDto
            {
                Token = token,
                // Agrega esta propiedad a tu LoginResponseDto si no existe
                User = new UserDto
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol.ToString(),
                    FotoPerfilUrl = fotoUrlCompleta,
                    FechaCreacion = usuario.FechaCreacion,
                    // AÑADE ESTAS DOS LÍNEAS 👇
                    AreaId = usuario.AreaId,
                    AreaNombre = usuario.Area?.Nombre
                }
            };
        }

        public async Task LogoutAsync()
        {
            // 1. Obtenemos el ID del usuario actual usando el HttpContextAccessor que ya tienes inyectado
            var user = _httpContextAccessor.HttpContext?.User;

            // Buscamos el claim "sub" (Subject) o NameIdentifier que guardamos en el token
            var userIdStr = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? user?.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                // 2. Registramos el evento
                await _eventRepository.AddAsync(new SystemEvent
                {
                    UsuarioId = userId,
                    Tipo = Core.Domain.Enums.SystemEventType.Logout,
                    Detalles = "Cierre de sesión voluntario",
                    Fecha = DateTime.UtcNow
                });
            }
        }

        // =====================================================
        // JWT
        // =====================================================
        private string GenerateJwtToken(Usuario usuario)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key no configurado.");

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expireMinutes = int.TryParse(
                _configuration["Jwt:ExpireMinutes"],
                out var minutes
            ) ? minutes : 60;

            var claims = new List<Claim>
{
    // Usamos estándares JWT: 'sub' es el ID del usuario
    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    
    // Claims personalizados con nombres cortos
    new Claim("email", usuario.Email),
    new Claim("nombre", usuario.Nombre),
    new Claim("role", usuario.Rol.ToString()),
    new Claim("areaId", usuario.AreaId?.ToString() ?? string.Empty)
};

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
