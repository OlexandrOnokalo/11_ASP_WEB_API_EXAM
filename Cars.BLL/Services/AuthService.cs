using Cars.BLL.Dtos.Auth;
using Cars.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Cars.BLL.Services
{
    // Вся бізнес-логіка реєстрації, входу і підтвердження email — тут.
    // Identity робить важку роботу (хешування, збереження), я тільки оркеструю.
    public class AuthService
    {
        private readonly UserManager<AppUserEntity> _userManager;
        private readonly RoleManager<AppRoleEntity> _roleManager;
        private readonly JwtService _jwtService;

        public AuthService(
            UserManager<AppUserEntity> userManager,
            RoleManager<AppRoleEntity> roleManager,
            JwtService jwtService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        // Точка входу реєстрації — перевіряю унікальність окремо, бо Identity кидає загальну помилку
        // без вказівки що саме зайнято (email чи username).
        public async Task<RegisterResultDto> RegisterAsync(RegisterDto dto)
        {
            // Перевіряю email і username окремо, щоб дати юзеру точне повідомлення про помилку
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
            {
                throw new InvalidOperationException($"Пошта '{dto.Email}' вже використовується.");
            }

            if (await _userManager.FindByNameAsync(dto.UserName) != null)
            {
                throw new InvalidOperationException($"Ім'я користувача '{dto.UserName}' зайняте.");
            }

            var user = new AppUserEntity
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(createResult.Errors.First().Description);
            }

            // Роль "user" сіється через Seeder, але на випадок якщо сідер не відпрацював —
            // створюю на льоту, щоб реєстрація не впала.
            if (!await _roleManager.RoleExistsAsync("user"))
            {
                await _roleManager.CreateAsync(new AppRoleEntity { Name = "user" });
            }

            await _userManager.AddToRoleAsync(user, "user");

            // Identity генерує токен з '+', '/' — URL-кодую, бо він піде як query-параметр
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string encodedToken = Uri.EscapeDataString(token);

            return new RegisterResultDto
            {
                Message = "Реєстрація успішна. Підтвердіть email через /api/auth/confirm-email",
                UserId = user.Id,
                ConfirmationToken = encodedToken
            };
        }

        // Точка входу логіну — шукаю за email (не username), бо так вирішив у RegisterDto.
        public async Task<AuthResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new InvalidOperationException($"Користувач з поштою '{dto.Email}' не існує.");
            }

            bool passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                throw new InvalidOperationException("Пароль вказано невірно.");
            }

            // Генерацію токенів делегую JwtService — він же зберігає refresh у БД
            var tokens = await _jwtService.GenerateTokensAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResultDto
            {
                Tokens = tokens,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToArray()
                }
            };
        }

        // Зворотній бік RegisterAsync — декодую токен, бо при реєстрації я його закодував
        public async Task ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Користувача не знайдено.");
            }

            // Розкодовую URL-encoding, що зробив при генерації токена в RegisterAsync
            string decodedToken = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Errors.First().Description);
            }
        }
    }
}