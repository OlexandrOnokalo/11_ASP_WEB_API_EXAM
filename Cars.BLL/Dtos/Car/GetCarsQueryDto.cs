namespace Cars.BLL.Dtos.Car
{
    // DTO — параметри GET /; Property+Value — один фільтр за раз, обидва не підтримую навмисно.
    public class GetCarsQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;

        public string? Property { get; set; }
        public string? Value { get; set; }
    }
}