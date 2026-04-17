namespace Cars.API.Models
{
    public class ErrorResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}