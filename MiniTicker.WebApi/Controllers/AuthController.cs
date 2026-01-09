using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using MiniTicker.Core.Application.Interfaces.Services;

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
    }
}
