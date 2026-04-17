using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Manufacture
{
    public class CreateManufactureDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}