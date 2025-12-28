using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Application.Interfaces.Services;
using System.IO; // Necesario para Path y Directory

namespace MiniTicker.Infrastructure.Persistence.Services
{
    public class FileStorageService : IFileStorageService
    {
        // Inyectamos IWebHostEnvironment para obtener la ruta correcta de wwwroot (opcional, pero recomendado)
        // Si no quieres inyectar nada, usaremos Directory.GetCurrentDirectory() como abajo:

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            // 1. Definir la ruta física: TuProyecto/wwwroot/uploads/carpeta
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);

            // 2. Crear la carpeta si no existe
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 3. Generar un nombre único para evitar colisiones
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            // 4. Guardar el archivo físicamente
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Retornar la ruta relativa URL (usando / en lugar de \)
            // El navegador buscará en: https://tusitio/uploads/usuarios/foto.jpg
            return $"uploads/{folder}/{fileName}";
        }

        public async Task DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileUrl.Replace("/", "\\"));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await Task.CompletedTask;
        }
    }
}