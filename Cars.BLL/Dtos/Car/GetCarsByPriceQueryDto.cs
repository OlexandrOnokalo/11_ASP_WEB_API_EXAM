namespace Cars.BLL.Dtos.Car
{
    // DTO — параметри GET /by-price; min/max можна переплутати — CarService поміняє сам.
    public class GetCarsByPriceQueryDto
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }
}