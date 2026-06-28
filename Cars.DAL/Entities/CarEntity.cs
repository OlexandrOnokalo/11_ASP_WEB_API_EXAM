namespace Cars.DAL.Entities
{
    public class CarEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        // FK + навігаційна властивість — завантажую через Include там де потрібно
        public int ManufactureId { get; set; }
        public ManufactureEntity? Manufacture { get; set; }

        public int Year { get; set; }
        // decimal, бо в БД маплю на numeric(p,s) — без float-похибок для грошей та об'єму
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public required string Color { get; set; }

        // Опціональні поля — не обов'язкові при створенні
        public string? Description { get; set; }
        // Зберігаю URL, а не бінарник — файл лежить в Storage/Cars/
        public string? Image { get; set; }
    }
}