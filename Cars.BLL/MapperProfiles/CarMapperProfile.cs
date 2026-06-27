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
                // навігаційне проперті — вказую явно, бо AutoMapper не знаходить неставлений об’єкт автоматично
                .ForMember(dest => dest.Manufacture, opt => opt.MapFrom(src => src.Manufacture));

            CreateMap<CreateCarDto, CarEntity>()
                // Image обробляється окремо через ImageService — якщо не Ignore(), AutoMapper покладе null
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                // опечатка у вхідному DTO: поле називається Desciption — приймаю обидва варіанти для сумісності з клієнтами
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? src.Desciption));

            CreateMap<UpdateCarDto, CarEntity>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? src.Desciption));
        }
    }
}