namespace Cars.DAL.Entities
{
    public class ManufactureEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public List<CarEntity> Cars { get; set; } = [];
    }
}