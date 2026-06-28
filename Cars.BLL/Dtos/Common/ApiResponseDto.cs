namespace Cars.BLL.Dtos.Common
{
    // DTO — універсальна обгортка відповіді; фронт завжди отримує { data: ... } а не сирий об'єкт.
    public class ApiResponseDto<T>
    {
        public T Data { get; set; } = default!;
    }
}