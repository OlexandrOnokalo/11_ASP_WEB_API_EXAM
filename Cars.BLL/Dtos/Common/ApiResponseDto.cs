namespace Cars.BLL.Dtos.Common
{
    public class ApiResponseDto<T>
    {
        public T Data { get; set; } = default!;
    }
}