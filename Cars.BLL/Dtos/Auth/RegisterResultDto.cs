namespace Cars.BLL.Dtos.Auth
{
    // DTO — відповідь на реєстрацію; ConfirmationToken повертаю прямо (в реальному проекті мав би йти на електронну пошту).
    public class RegisterResultDto
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ConfirmationToken { get; set; } = string.Empty;
    }
}