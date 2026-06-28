using AutoMapper;
using Cars.BLL.Dtos.Car;
using Cars.BLL.Dtos.Common;
using Cars.DAL;
using Cars.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cars.BLL.Services
{
    // CRUD для авто: пагінація, фільтрація, управління зображеннями.
    // Напряму з AppDbContext — репозиторій тут не виносив, бо логіки запитів мало.
    public class CarService
    {
        // Дефолтні зображення — логотипи брендів, які лежать у статиці.
        // Захищаю їх від видалення при оновленні/видаленні авто.
        private const string ToyotaDefaultImage = "/images/cars/Toyota-logo.jpg";
        private const string BmwDefaultImage = "/images/cars/BMW-logo.png";
        private const string AudiDefaultImage = "/images/cars/audi-logo.png";
        private const string CarsImagesUrl = "/images/cars";

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ImageService _imageService;

        public CarService(AppDbContext context, IMapper mapper, ImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        // Вхідна точка отримання списку авто — пагінація + опціональний фільтр.
        public async Task<PagedDataDto<CarItemDto>> GetAllAsync(GetCarsQueryDto queryDto)
        {
            int page = queryDto.Page;
            int pageSize = queryDto.PageSize;
            // Нормалізую до виконання запиту, щоб не отримати від'ємний Skip або гігантський Take
            NormalizePaging(ref page, ref pageSize);

            var query = _context.Cars
                .AsNoTracking()
                .Include(x => x.Manufacture)
                .AsQueryable();

            // Фільтр опціональний — якщо property/value порожні, повертає все
            query = ApplyFilter(query, queryDto.Property, queryDto.Value);

            // CountAsync до Skip/Take — рахую повну кількість для фронту (totalPages)
            var totalCount = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<CarItemDto>>(entities);

            return new PagedDataDto<CarItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // Окремий метод для цінового діапазону — не став засмічувати ApplyFilter decimal-логікою.
        public async Task<PagedDataDto<CarItemDto>> GetByPriceAsync(GetCarsByPriceQueryDto queryDto)
        {
            int page = queryDto.Page;
            int pageSize = queryDto.PageSize;
            NormalizePaging(ref page, ref pageSize);

            decimal min = queryDto.MinValue;
            decimal max = queryDto.MaxValue;

            // Якщо юзер переплутав min і max — мовчки виправляю, не кидаю помилку
            if (min > max)
            {
                (min, max) = (max, min);
            }

            var query = _context.Cars
                .AsNoTracking()
                .Include(x => x.Manufacture)
                .Where(x => x.Price >= min && x.Price <= max);

            var totalCount = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<CarItemDto>>(entities);

            return new PagedDataDto<CarItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CarItemDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Cars
                .AsNoTracking()
                .Include(x => x.Manufacture)
                .FirstOrDefaultAsync(x => x.Id == id);

            return entity == null ? null : _mapper.Map<CarItemDto>(entity);
        }

        // Вхідна точка створення авто — маппінг, дефолтне фото, збереження файлу.
        public async Task<CarItemDto> CreateAsync(CreateCarDto dto, string imagesPath)
        {
            bool manufactureExists = await _context.Manufactures.AnyAsync(x => x.Id == dto.ManufactureId);
            if (!manufactureExists)
            {
                throw new ArgumentException("Manufacture does not exist.");
            }

            var entity = _mapper.Map<CarEntity>(dto);
            entity.Name = dto.Name.Trim();
            entity.Color = dto.Color.Trim();
            // dto.Desciption — навмисна опечатка в DTO (legacy), підтримую обидва поля
            entity.Description = (dto.Description ?? dto.Desciption)?.Trim();
            // Спочатку ставлю дефолтне фото по бренду, потім перезаписую якщо прийшов файл
            entity.Image = await GetDefaultImageByManufactureIdAsync(dto.ManufactureId);

            if (dto.Image != null)
            {
                entity.Image = await _imageService.SaveAsync(dto.Image, imagesPath, CarsImagesUrl);
            }

            _context.Cars.Add(entity);
            await _context.SaveChangesAsync();

            // Перечитую з Include(Manufacture) — щоб повернути повний DTO з вкладеним об'єктом
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<CarItemDto?> UpdateAsync(int id, UpdateCarDto dto, string imagesPath)
        {
            var entity = await _context.Cars.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return null;
            }

            bool manufactureExists = await _context.Manufactures.AnyAsync(x => x.Id == dto.ManufactureId);
            if (!manufactureExists)
            {
                throw new ArgumentException("Manufacture does not exist.");
            }

            // Запам'ятовую старе фото до маппінгу — AutoMapper перезапише поле
            string? oldImage = entity.Image;

            _mapper.Map(dto, entity);

            entity.Name = dto.Name.Trim();
            entity.Color = dto.Color.Trim();
            entity.Description = (dto.Description ?? dto.Desciption)?.Trim();

            if (dto.Image != null)
            {
                // Прийшов новий файл — видаляю старий (якщо не дефолтний) і зберігаю новий
                _imageService.DeleteIfExists(oldImage, imagesPath, ToyotaDefaultImage, BmwDefaultImage, AudiDefaultImage);
                entity.Image = await _imageService.SaveAsync(dto.Image, imagesPath, CarsImagesUrl);
            }
            else if (IsDefaultImage(oldImage))
            {
                // Файл не прийшов, але бренд міг змінитись — оновлюю дефолтне фото під новий бренд
                entity.Image = await GetDefaultImageByManufactureIdAsync(dto.ManufactureId);
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteAsync(int id, string imagesPath)
        {
            var entity = await _context.Cars.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return false;
            }

            _imageService.DeleteIfExists(entity.Image, imagesPath, ToyotaDefaultImage, BmwDefaultImage, AudiDefaultImage);

            _context.Cars.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Підбираю дефолтне фото по назві бренду — тільки відомі бренди, решта отримує Toyota.
        // Якщо додати новий бренд у БД — тут треба допис, це відомий trade-off.
        private async Task<string> GetDefaultImageByManufactureIdAsync(int manufactureId)
        {
            string? name = await _context.Manufactures
                .AsNoTracking()
                .Where(x => x.Id == manufactureId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Manufacture does not exist.");
            }

            return name.Trim().ToLower() switch
            {
                "toyota" => ToyotaDefaultImage,
                "bmw" => BmwDefaultImage,
                "audi" => AudiDefaultImage,
                // Невідомий бренд — даю Toyota як запасний варіант
                _ => ToyotaDefaultImage
            };
        }

        // Перевіряю чи URL є одним із захищених дефолтних — такі файли не можна видаляти з диску
        private static bool IsDefaultImage(string? imageUrl)
        {
            return string.Equals(imageUrl, ToyotaDefaultImage, StringComparison.OrdinalIgnoreCase)
                || string.Equals(imageUrl, BmwDefaultImage, StringComparison.OrdinalIgnoreCase)
                || string.Equals(imageUrl, AudiDefaultImage, StringComparison.OrdinalIgnoreCase);
        }

        // Фільтр через IQueryable — все транслюється в SQL, не тягну зайвого в пам'ять.
        // Невідоме поле — ігнорую фільтр, не кидаю помилку (м'яка поведінка для фронту).
        private static IQueryable<CarEntity> ApplyFilter(IQueryable<CarEntity> query, string? property, string? value)
        {
            if (string.IsNullOrWhiteSpace(property) || string.IsNullOrWhiteSpace(value))
            {
                return query;
            }

            string p = property.Trim().ToLower();
            string v = value.Trim();

            return p switch
            {
                "name" => query.Where(x => x.Name.ToLower().Contains(v.ToLower())),
                "manufacture" => query.Where(x => x.Manufacture != null && x.Manufacture.Name.ToLower().Contains(v.ToLower())),
                // Якщо year/volume не парсяться — повертаю порожній результат, а не все підряд
                "year" => int.TryParse(v, out int year) ? query.Where(x => x.Year == year) : query.Where(_ => false),
                "color" => query.Where(x => x.Color.ToLower().Contains(v.ToLower())),
                "volume" => decimal.TryParse(v, out decimal volume) ? query.Where(x => x.Volume == volume) : query.Where(_ => false),
                _ => query
            };
        }

        // Захищаю від некоректних значень пагінації — 500 як стеля, щоб не вбити БД одним запитом
        private static void NormalizePaging(ref int page, ref int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 500) pageSize = 500;
        }
    }
}