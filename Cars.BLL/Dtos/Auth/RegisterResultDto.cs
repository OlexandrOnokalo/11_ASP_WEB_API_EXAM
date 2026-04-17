namespace Cars.BLL.Dtos.Auth
{
    public class RegisterResultDto
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ConfirmationToken { get; set; } = string.Empty;
    }
}