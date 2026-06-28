using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Manufacture
{
    // DTO — вхід PUT; Id в тілі запиту має збігатись з id з route — ManufactureService перевіряє.
    public class UpdateManufactureDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}