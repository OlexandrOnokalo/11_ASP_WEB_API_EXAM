using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Car
{
    // DTO — вхід POST [FromForm]: є IFormFile, тому multipart/form-data, не JSON.
    public class CreateCarDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int ManufactureId { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        [Range(0.01, 1000)]
        public decimal Volume { get; set; }

        [Required]
        [Range(0, 1000000000)]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(40)]
        public string Color { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Тримаю обидва поля: фронт надсилає Desciption (опечатка), видаляти не можу — зламаю клієнта
        public string? Desciption { get; set; }

        public IFormFile? Image { get; set; }
    }
}