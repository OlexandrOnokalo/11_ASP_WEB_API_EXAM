using AutoMapper;
using Cars.BLL.Dtos.Car;
using Cars.DAL.Entities;

namespace Cars.BLL.MapperProfiles
{
    public class CarMapperProfile : Profile
    {
        public CarMapperProfile()
        {
            CreateMap<ManufactureEntity, CarManufactureDto>();

            CreateMap<CarEntity, CarItemDto>()
                .ForMember(dest => dest.Manufacture, opt => opt.MapFrom(src => src.Manufacture));

            CreateMap<CreateCarDto, CarEntity>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? src.Desciption));

            CreateMap<UpdateCarDto, CarEntity>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? src.Desciption));
        }
    }
}