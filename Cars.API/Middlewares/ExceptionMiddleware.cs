using Cars.API.Models;

namespace Cars.API.Middlewares
{
    // Централізована обробка помилок — будь-який неперехоплений виняток зі сервісів/репозиторії
    // потрапляє сюди і перетворюється на JSON-відповідь замість стандартного HTML 500
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            // ArgumentException та InvalidOperationException — очікувані бізнес-помилки (email зайнятий,
            // виробник не знайшов тощо) — повертаю 400 і логую як Warning, бо це не наша помилка
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto { Message = ex.Message });
            }
            // Будь-яке інше — це вже несподіване, повертаю 500 без деталей —
            // stack trace назовні не віддаю, тільки в логи
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto { Message = "Internal server error." });
            }
        }
    }
}