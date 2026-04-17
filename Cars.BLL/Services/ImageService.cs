using Microsoft.AspNetCore.Http;

namespace Cars.BLL.Services
{
    public class ImageService
    {
        public async Task<string> SaveAsync(IFormFile file, string imagesPath, string requestPath)
        {
            if (file.Length == 0)
            {
                throw new ArgumentException("Файл зображення порожній.");
            }

            Directory.CreateDirectory(imagesPath);

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(imagesPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"{requestPath}/{fileName}";
        }

        public void DeleteIfExists(string? imageUrl, string imagesPath, params string[] protectedUrls)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            if (protectedUrls.Any(x => string.Equals(x, imageUrl, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            string fileName = Path.GetFileName(imageUrl);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            string fullPath = Path.Combine(imagesPath, fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}