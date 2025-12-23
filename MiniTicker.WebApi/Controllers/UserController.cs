using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Users;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _userService.GetAllAsync().ConfigureAwait(false);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id).ConfigureAwait(false);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto dto)
        {
            if (dto == null) return BadRequest();

            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var updated = await _userService.UpdateProfileAsync(userId, dto).ConfigureAwait(false);
            return Ok(updated);
        }

        [HttpPut("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            await _userService.ActivateAsync(id).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPut("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await _userService.DeactivateAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
