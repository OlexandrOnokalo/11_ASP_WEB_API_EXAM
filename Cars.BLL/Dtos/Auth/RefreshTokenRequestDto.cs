using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}