using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Domain.Enums;
using System;

namespace MiniTicker.Core.Application.Users
{
    public class UpdateUserDto // Le cambié el nombre a algo más genérico
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Opcional: Si viene vacío, mantenemos la contraseña vieja
        public string? Password { get; set; }

        public Rol Rol { get; set; }
        public Guid? AreaId { get; set; }
        public bool Activo { get; set; }

        public IFormFile? FotoPerfil { get; set; }
    }
}