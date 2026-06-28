namespace Cars.BLL.Dtos.Common
{
    // DTO — стандартна структура для всіх пагінованих списків; TotalCount нужний фронту для розрахунку кількості сторінок.
    public class PagedDataDto<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}