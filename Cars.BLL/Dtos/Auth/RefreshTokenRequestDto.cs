using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Auth
{
    // DTO — тіло запиту POST /api/auth/refresh.
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}