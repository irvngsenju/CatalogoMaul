using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogoMaui.Services
{
    public static class FotosProducto
    {
        private const long MaxBytes = 15L * 1024 * 1024;
        private const string CarpetaFotos = "fotos_productos";
    

    public static async Task<string?> ElegirAsync()
        {
            var resultado = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Selecciona una foto (JPG o PNG)"
            });

            if (resultado is null)
                return null;

            var ext = Path.GetExtension(resultado.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png"))
                throw new InvalidOperationException("Solo se permiten imágenes JPG o PNG.");

            await using var origen = await resultado.OpenReadAsync();
            if (origen.Length > MaxBytes)
                throw new InvalidOperationException("La imagen no debe superar los 15 MB.");

            Directory.CreateDirectory(FileSystem.CacheDirectory);
            var rutaTemporal = Path.Combine(
                FileSystem.CacheDirectory,
                $"preview_{Guid.NewGuid():N}{ext}");

            await using var destino = File.Create(rutaTemporal);
            await origen.CopyToAsync(destino);

            return rutaTemporal;
        }

        public static async Task<string> GuardarAsync(string rutaTemporal)
        {
            var ext = Path.GetExtension(rutaTemporal).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
                ext = ".jpg";

            var carpetaDestino = Path.Combine(FileSystem.AppDataDirectory, CarpetaFotos);
            Directory.CreateDirectory(carpetaDestino);

            var nombreArchivo = $"producto_{Guid.NewGuid():N}{ext}";
            var rutaFinal = Path.Combine(carpetaDestino, nombreArchivo);

            await using var origen = File.OpenRead(rutaTemporal);
            await using var destino = File.Create(rutaFinal);
            await origen.CopyToAsync(destino);

            return nombreArchivo;
        }

        public static string Resolver(string? foto)
        {
            if (string.IsNullOrWhiteSpace(foto))
                return "";

            if (Path.IsPathRooted(foto) || foto.Contains('/') || foto.Contains('\\'))
                return foto;

            var rutaPropia = Path.Combine(FileSystem.AppDataDirectory, CarpetaFotos, foto);
            return File.Exists(rutaPropia) ? rutaPropia : foto;
        }

        public static void EliminarSiEsPropia(string? foto)
        {
            if (string.IsNullOrWhiteSpace(foto) || !foto.StartsWith("producto", StringComparison.OrdinalIgnoreCase))
                return;

            var rutaPropia = Path.Combine(FileSystem.AppDataDirectory, CarpetaFotos, foto);
            if (File.Exists(rutaPropia))
                File.Delete(rutaPropia);
        }

        public static void EliminarTemporal(string? rutaTemporal)
        {
            if (string.IsNullOrWhiteSpace(rutaTemporal))
                return;

            if (rutaTemporal.StartsWith(FileSystem.CacheDirectory, StringComparison.OrdinalIgnoreCase) &&
                File.Exists(rutaTemporal))
            {
                File.Delete(rutaTemporal);
            }
        }
    }
}
