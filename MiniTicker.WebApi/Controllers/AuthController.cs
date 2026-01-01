using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Auth;
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
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null) return BadRequest();

            try
            {
                var response = await _authService.LoginAsync(request).ConfigureAwait(false);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}
