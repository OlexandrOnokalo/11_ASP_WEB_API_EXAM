namespace Cars.DAL.Entities
{
    public class ManufactureEntity
    {
        public int Id { get; set; }
        // В БД є унікальний індекс на Name — перевіряю це ще й в сервісі, щоб дати зрозуміле повідомлення
        public required string Name { get; set; }

        // Навігаційна колекція — не підвантажується автоматично, тільки якщо явно Include або через Restrict при видаленні
        public List<CarEntity> Cars { get; set; } = [];
    }
}