namespace Cars.BLL.Dtos.Auth
{
    public class AuthResultDto
    {
        public JwtDto Tokens { get; set; } = new();
        public AuthUserDto User { get; set; } = new();
    }
}