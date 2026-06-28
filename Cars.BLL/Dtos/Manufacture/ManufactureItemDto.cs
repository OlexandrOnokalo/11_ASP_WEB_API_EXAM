namespace Cars.BLL.Dtos.Manufacture
{
    // DTO — відповідь на GET; використовую також як вкладений об'єкт в CarItemDto.
    public class ManufactureItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}