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

            // не кидає помилку якщо директорія вже існує — зручно гарантую наявність
            Directory.CreateDirectory(imagesPath);

            string extension = Path.GetExtension(file.FileName);
            // GUID — унікальне ім’я щоб не було колізій; розширення зберігаю, бо browser цікавиться на MIME-тип
            string fileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(imagesPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // повертаю URL для збереження в БД, а не фізичний шлях — клієнт звертається за цим URL
            return $"{requestPath}/{fileName}";
        }

        public void DeleteIfExists(string? imageUrl, string imagesPath, params string[] protectedUrls)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            // дефолтні логотипи ніколи не видаляються — передаються ззовні через params
            if (protectedUrls.Any(x => string.Equals(x, imageUrl, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            // в БД зберігаю URL, а не шлях — витягую лише ім’я файлу і складаю фізичний шлях самостійно
            string fileName = Path.GetFileName(imageUrl);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            string fullPath = Path.Combine(imagesPath, fileName);
            // File.Delete кидає виняток якщо файл не існує — тому перевіряю заранісь
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}