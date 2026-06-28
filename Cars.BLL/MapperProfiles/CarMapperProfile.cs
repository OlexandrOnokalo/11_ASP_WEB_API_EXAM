using AutoMapper;
using Cars.BLL.Dtos.Car;
using Cars.DAL.Entities;

namespace Cars.BLL.MapperProfiles
{
    // Реєструю AutoMapper через AddAutoMapper(CarMapperProfile.Assembly) у Program.cs.
    // Тут всі маппінги пов'язані з авто — Entity↔DTO і DTO→Entity.
    public class CarMapperProfile : Profile
    {
        public CarMapperProfile()
        {
            // Простий маппінг по іменах — використовую коли маплю авто включаючи Manufacture
            CreateMap<ManufactureEntity, CarManufactureDto>();

            // Навігаційне поле треба вказати явно — AutoMapper не мапає вкладені об'єкти автоматично
            CreateMap<CarEntity, CarItemDto>()
                .ForMember(dest => dest.Manufacture, opt => opt.MapFrom(src => src.Manufacture));

            // Image ігнорую: його зберігає ImageService окремо після маппінгу.
            // Description ?? Desciption — підтримую обидва поля: вірне + legacy з опечаткою
            CreateMap<CreateCarDto, CarEntity>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? src.Desciption));

            CreateMap<UpdateCarDto, CarEntity>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? src.Desciption));
        }
    }
}