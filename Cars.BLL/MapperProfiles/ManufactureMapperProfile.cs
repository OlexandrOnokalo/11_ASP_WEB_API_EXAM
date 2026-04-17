using AutoMapper;
using Cars.BLL.Dtos.Manufacture;
using Cars.DAL.Entities;

namespace Cars.BLL.MapperProfiles
{
    public class ManufactureMapperProfile : Profile
    {
        public ManufactureMapperProfile()
        {
            CreateMap<ManufactureEntity, ManufactureItemDto>();
            CreateMap<CreateManufactureDto, ManufactureEntity>();
            CreateMap<UpdateManufactureDto, ManufactureEntity>();
        }
    }
}