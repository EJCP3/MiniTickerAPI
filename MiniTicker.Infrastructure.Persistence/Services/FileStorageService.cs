using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Application.Interfaces.Services;
using System.IO; 

namespace MiniTicker.Infrastructure.Persistence.Services
{
    public class FileStorageService : IFileStorageService
    {
        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            // 1. Definir la ruta física
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);

            // 2. Crear la carpeta si no existe
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // --- CAMBIO AQUÍ ---
            // 3. Generar nombre: "nombreOriginal_GUIDCorto.ext"
            
            // A. Obtenemos el nombre original sin la extensión (ej: "Mi Foto.jpg" -> "Mi Foto")
            string nombreOriginal = Path.GetFileNameWithoutExtension(file.FileName);
            
            // B. Limpiamos espacios para evitar problemas en URLs (ej: "Mi_Foto")
            string nombreLimpio = nombreOriginal.Replace(" ", "_");
            
            // C. Obtenemos la extensión (ej: ".jpg")
            string extension = Path.GetExtension(file.FileName);

            // D. Creamos el nombre final combinando todo con un GUID corto (8 chars)
            // Resultado: "Mi_Foto_a1b2c3d4.jpg"
            var fileName = $"{nombreLimpio}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
            // -------------------

            var filePath = Path.Combine(folderPath, fileName);

            // 4. Guardar el archivo físicamente
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Retornar la ruta relativa URL
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