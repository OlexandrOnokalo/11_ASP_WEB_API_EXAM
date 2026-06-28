namespace Cars.BLL.Dtos.Auth
{
    // DTO — мінімальний профіль юзера для фронту: ідентифікація + ролі для праврування доступом.
    public class AuthUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
    }
}