namespace Cars.BLL.Dtos.Car
{
    // DTO — мінімальний виробник вкладений в CarItemDto, щоб фронт не робив окремий запит по ManufactureId.
    public class CarManufactureDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}