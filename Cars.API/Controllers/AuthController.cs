using Cars.BLL.Dtos.Auth;
using Cars.BLL.Dtos.Common;
using Cars.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cars.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        // JwtService є окремо — refresh це операція над токеном, а не бізнес-логіка усера;
        // вирішив не мішати це в AuthService
        private readonly JwtService _jwtService;

        public AuthController(AuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(new ApiResponseDto<RegisterResultDto> { Data = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(new ApiResponseDto<AuthResultDto> { Data = result });
        }

        // Refresh іде через JwtService — перевіряє чи токен не протермінований і не використаний,
        // потім видає нову пару access+refresh — одноразове використання (rotation)
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _jwtService.RefreshAsync(dto.RefreshToken);
            return Ok(new ApiResponseDto<JwtDto> { Data = result });
        }

        // GET з querystring — бо посилання підтвердження приходить лінком у листі;
        // userId та token це параметри URL, а не тіло запиту
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string token)
        {
            await _authService.ConfirmEmailAsync(userId, token);
            return Ok(new ApiResponseDto<object> { Data = new { message = "Email успішно підтверджено." } });
        }
    }
}