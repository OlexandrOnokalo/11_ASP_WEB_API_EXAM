using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Auth
{
    // DTO — вхід; вход саме по email, бо юзер може забути username, а пошту — ні.
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}