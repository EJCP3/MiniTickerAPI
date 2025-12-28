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
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IConfiguration configuration)
        {
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher
                ?? throw new ArgumentNullException(nameof(passwordHasher));
            _configuration = configuration
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        // =====================================================
        // LOGIN
        // =====================================================
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Email y contraseña son requeridos.");

            var usuario = await _userRepository.GetByEmailAsync(dto.Email);

            if (usuario == null || !usuario.Activo)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            // ✅ Verificación segura con BCrypt
            if (!_passwordHasher.Verify(dto.Password, usuario.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

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
    // Usamos estándares JWT: 'sub' es el ID del usuario
    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    
    // Claims personalizados con nombres cortos
    new Claim("email", usuario.Email),
    new Claim("nombre", usuario.Nombre),
    new Claim("role", usuario.Rol.ToString())
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
