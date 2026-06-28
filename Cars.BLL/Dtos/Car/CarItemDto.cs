namespace Cars.BLL.Dtos.Car
{
    // DTO — відповідь на GET: повні дані авто + вкладений об'єкт виробника.
    public class CarItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ManufactureId { get; set; }
        public int Year { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public string Color { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }

        // null якщо запит без Include — в сервісі завжди роблю Include
        public CarManufactureDto? Manufacture { get; set; }
    }
}