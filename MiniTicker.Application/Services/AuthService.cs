using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniTicker.Core.Application.Auth;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Core.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email requerido.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password requerido.", nameof(dto));

            var usuario = await _userRepository.GetByEmailAsync(dto.Email);
            if (usuario == null || !usuario.Activo)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            // ⚠️ TEMPORAL: comparación simple
            // En producción: usar PasswordHash + BCrypt
            var passwordProp =
                usuario.GetType().GetProperty("PasswordHash") ??
                usuario.GetType().GetProperty("Password");

            if (passwordProp != null)
            {
                var stored = passwordProp.GetValue(usuario) as string;
                if (string.IsNullOrEmpty(stored) || stored != dto.Password)
                    throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            var token = GenerateJwtToken(usuario);

            return new LoginResponseDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                Rol = usuario.Rol.ToString()
            };
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
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Rol.ToString())
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
