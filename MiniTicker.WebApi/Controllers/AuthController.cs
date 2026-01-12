
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.DTOs.Auth;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Tags("01. Auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message == "USER_LOCKED")
            {
                // Devolvemos 403 Forbidden con el mensaje real
                return StatusCode(403, new { message = "Tu cuenta está desactivada. Contacta al administrador." });
            }
            catch (UnauthorizedAccessException)
            {
                // Devolvemos 401 con el mensaje genérico
                return Unauthorized(new { message = "Email o contraseña incorrectos." });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Delegamos la lógica al servicio, manteniendo el controlador limpio
            await _authService.LogoutAsync();

            return Ok(new { message = "Sesión cerrada registrada" });
        }

        [HttpPost("complete-setup")]
        [Authorize]
        public async Task<IActionResult> CompleteSetup([FromForm] InitialSetupRequest request)
        {
            // Obtener ID del token de forma segura
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("uid")?.Value;

            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            // Llamamos al servicio, NO al context directamente
            var fotoUrl = await _authService.CompleteSetupAsync(userId, request);

            return Ok(new { fotoUrl });
        }
    }
}
