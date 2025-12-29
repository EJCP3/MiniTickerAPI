using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Users;

namespace MiniTicker.WebApi.Controllers
{
    [ApiController]
    // [Authorize(Roles = "SuperAdmin")] 
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var items = await _userService.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateUserDto dto, CancellationToken cancellationToken)
        {
            if (dto == null) return BadRequest("Datos inválidos.");
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("El email es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Password)) return BadRequest("La contraseña es obligatoria.");

            var created = await _userService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromForm] UpdateUserDto dto)
        {
            var result = await _userService.UpdateAsync(id, dto);
            return Ok(result);
        }


        [HttpPut("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        {
            await _userService.ActivateAsync(id, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPut("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        {
            await _userService.DeactivateAsync(id, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _userService.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            return NoContent(); // Retorna 204 si se borró correctamente
        }
    }
}
