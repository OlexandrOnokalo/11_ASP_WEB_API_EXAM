using Microsoft.AspNetCore.Http;

namespace Cars.BLL.Services
{
    // Роботає тільки з фізичними файлами на диску — запис URL в БД не його забота.
    public class ImageService
    {
        // Зберігаю файл і повертаю URL, доступний через UseStaticFiles.
        // imagesPath — фізичний шлях (Storage/Cars), requestPath — URL-префікс (/images/cars).
        public async Task<string> SaveAsync(IFormFile file, string imagesPath, string requestPath)
        {
            if (file.Length == 0)
            {
                throw new ArgumentException("Файл зображення порожній.");
            }

            // Ідемпотентно — не кидає виняток якщо папка вже існує
            Directory.CreateDirectory(imagesPath);

            // Guid як ім'я — завжди унікальне, не залежить від оригінального імені файлу
            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(imagesPath, fileName);

            // await using — гарантую закриття стріму навіть при помилці
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"{requestPath}/{fileName}";
        }

        // Видаляю фізичний файл, але не чіпаю запис в БД — це робить викликаючий.
        // protectedUrls — дефолтні логотипи брендів, які ніколи не видаляю.
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

            // imageUrl — це URL (/images/cars/abc.jpg), витягую тільки ім'я файлу для побудови шляху на диску
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