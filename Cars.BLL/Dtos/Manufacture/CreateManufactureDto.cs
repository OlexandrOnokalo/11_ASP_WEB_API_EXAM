using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Manufacture
{
    // DTO — вхід POST; лише Name — виробник це просто назва.
    public class CreateManufactureDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}