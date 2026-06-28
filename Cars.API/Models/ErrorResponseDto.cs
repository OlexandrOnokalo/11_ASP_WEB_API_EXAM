namespace Cars.API.Models
{
    // єдиний формат помилок для всього API — ExceptionMiddleware і InvalidModelStateResponseFactory використовують однаково
    public class ErrorResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; } // null при бізнес-помилках; заповнено тільки при validation errors
    }
}