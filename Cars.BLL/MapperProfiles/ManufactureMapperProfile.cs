using AutoMapper;
using Cars.BLL.Dtos.Manufacture;
using Cars.DAL.Entities;

namespace Cars.BLL.MapperProfiles
{
    // Маппінги для виробників — всі поля збігаються за іменами, тому жодних ForMember не потрібно.
    public class ManufactureMapperProfile : Profile
    {
        public ManufactureMapperProfile()
        {
            // Двосторонній читальний маппінг Entity↔DTO
            CreateMap<ManufactureEntity, ManufactureItemDto>();
            // DTO→Entity для створення і оновлення — Id у Update прийде з route, не з маппінгу
            CreateMap<CreateManufactureDto, ManufactureEntity>();
            CreateMap<UpdateManufactureDto, ManufactureEntity>();
        }
    }
}