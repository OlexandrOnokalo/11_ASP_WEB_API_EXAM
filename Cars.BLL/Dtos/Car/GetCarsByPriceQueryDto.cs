namespace Cars.BLL.Dtos.Car
{
    public class GetCarsByPriceQueryDto
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }
}