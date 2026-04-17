using Cars.BLL.Dtos.Car;
using Cars.BLL.Dtos.Common;
using Cars.DAL;
using Cars.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cars.BLL.Services
{
    public class CarService
    {
        private readonly AppDbContext _context;

        public CarService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedDataDto<CarItemDto>> GetAllAsync(GetCarsQueryDto queryDto)
        {
            int page = queryDto.Page;
            int pageSize = queryDto.PageSize;
            NormalizePaging(ref page, ref pageSize);

            var query = _context.Cars
                .AsNoTracking()
                .Include(x => x.Manufacture)
                .AsQueryable();

            query = ApplyFilter(query, queryDto.Property, queryDto.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToCarItemDtoExpression())
                .ToListAsync();

            return new PagedDataDto<CarItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedDataDto<CarItemDto>> GetByPriceAsync(GetCarsByPriceQueryDto queryDto)
        {
            int page = queryDto.Page;
            int pageSize = queryDto.PageSize;
            NormalizePaging(ref page, ref pageSize);

            decimal min = queryDto.MinValue;
            decimal max = queryDto.MaxValue;

            if (min > max)
            {
                (min, max) = (max, min);
            }

            var query = _context.Cars
                .AsNoTracking()
                .Include(x => x.Manufacture)
                .Where(x => x.Price >= min && x.Price <= max);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToCarItemDtoExpression())
                .ToListAsync();

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
            return await _context.Cars
                .AsNoTracking()
                .Include(x => x.Manufacture)
                .Where(x => x.Id == id)
                .Select(MapToCarItemDtoExpression())
                .FirstOrDefaultAsync();
        }

        public async Task<CarItemDto> CreateAsync(CreateCarDto dto)
        {
            bool manufactureExists = await _context.Manufactures.AnyAsync(x => x.Id == dto.ManufactureId);
            if (!manufactureExists)
            {
                throw new ArgumentException("Manufacture does not exist.");
            }

            var entity = new CarEntity
            {
                Name = dto.Name.Trim(),
                ManufactureId = dto.ManufactureId,
                Year = dto.Year,
                Volume = dto.Volume,
                Price = dto.Price,
                Color = dto.Color.Trim(),
                Description = dto.Description?.Trim(),
                Image = dto.Image?.Trim()
            };

            _context.Cars.Add(entity);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<CarItemDto?> UpdateAsync(int id, UpdateCarDto dto)
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

            entity.Name = dto.Name.Trim();
            entity.ManufactureId = dto.ManufactureId;
            entity.Year = dto.Year;
            entity.Volume = dto.Volume;
            entity.Price = dto.Price;
            entity.Color = dto.Color.Trim();
            entity.Description = dto.Description?.Trim();
            entity.Image = dto.Image?.Trim();

            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Cars.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return false;
            }

            _context.Cars.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

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
                "year" => int.TryParse(v, out int year)
                    ? query.Where(x => x.Year == year)
                    : query.Where(_ => false),
                "color" => query.Where(x => x.Color.ToLower().Contains(v.ToLower())),
                "volume" => decimal.TryParse(v, out decimal volume)
                    ? query.Where(x => x.Volume == volume)
                    : query.Where(_ => false),
                _ => query
            };
        }

        private static System.Linq.Expressions.Expression<Func<CarEntity, CarItemDto>> MapToCarItemDtoExpression()
        {
            return x => new CarItemDto
            {
                Id = x.Id,
                Name = x.Name,
                ManufactureId = x.ManufactureId,
                Year = x.Year,
                Volume = x.Volume,
                Price = x.Price,
                Color = x.Color,
                Description = x.Description,
                Image = x.Image,
                Manufacture = x.Manufacture == null
                    ? null
                    : new CarManufactureDto
                    {
                        Id = x.Manufacture.Id,
                        Name = x.Manufacture.Name
                    }
            };
        }

        private static void NormalizePaging(ref int page, ref int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 500) pageSize = 500;
        }
    }
}