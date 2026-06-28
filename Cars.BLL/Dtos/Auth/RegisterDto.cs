using System.ComponentModel.DataAnnotations;

namespace Cars.BLL.Dtos.Auth
{
    // DTO — дані для реєстрації; FirstName/LastName необов'язкові — можна зареєструватись без них.
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}