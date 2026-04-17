namespace Cars.DAL.Entities
{
    public class CarEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public int ManufactureId { get; set; }
        public ManufactureEntity? Manufacture { get; set; }

        public int Year { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public required string Color { get; set; }

        public string? Description { get; set; }
        public string? Image { get; set; }
    }
}