using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Manufacture
{
    public class UpdateManufactureDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}