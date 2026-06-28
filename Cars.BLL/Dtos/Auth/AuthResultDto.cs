namespace Cars.BLL.Dtos.Auth
{
    // DTO — відповідь на успішний логін/рефреш: токени + профіль разом, щоб фронт зробив один запит.
    public class AuthResultDto
    {
        public JwtDto Tokens { get; set; } = new();
        public AuthUserDto User { get; set; } = new();
    }
}