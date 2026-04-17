using AutoMapper;
using Cars.BLL.Dtos.Common;
using Cars.BLL.Dtos.Manufacture;
using Cars.DAL;
using Cars.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cars.BLL.Services
{
    public class ManufactureService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ManufactureService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedDataDto<ManufactureItemDto>> GetAllAsync(int page, int pageSize)
        {
            NormalizePaging(ref page, ref pageSize);

            var query = _context.Manufactures.AsNoTracking().OrderBy(x => x.Id);

            var totalCount = await query.CountAsync();
            var entities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<ManufactureItemDto>>(entities);

            return new PagedDataDto<ManufactureItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ManufactureItemDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Manufactures
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return entity == null ? null : _mapper.Map<ManufactureItemDto>(entity);
        }

        public async Task<ManufactureItemDto> CreateAsync(CreateManufactureDto dto)
        {
            string name = dto.Name.Trim();

            bool exists = await _context.Manufactures
                .AnyAsync(x => x.Name.ToLower() == name.ToLower());

            if (exists)
            {
                throw new ArgumentException("Manufacture with this name already exists.");
            }

            var entity = _mapper.Map<ManufactureEntity>(dto);
            entity.Name = name;

            _context.Manufactures.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<ManufactureItemDto>(entity);
        }

        public async Task<ManufactureItemDto?> UpdateAsync(int id, UpdateManufactureDto dto)
        {
            if (dto.Id != id)
            {
                throw new ArgumentException("Route id and body id do not match.");
            }

            var entity = await _context.Manufactures.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return null;
            }

            string name = dto.Name.Trim();

            bool duplicate = await _context.Manufactures
                .AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower());

            if (duplicate)
            {
                throw new ArgumentException("Manufacture with this name already exists.");
            }

            _mapper.Map(dto, entity);
            entity.Name = name;

            await _context.SaveChangesAsync();

            return _mapper.Map<ManufactureItemDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Manufactures.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return false;
            }

            bool hasCars = await _context.Cars.AnyAsync(x => x.ManufactureId == id);
            if (hasCars)
            {
                throw new InvalidOperationException("Cannot delete manufacture that has cars.");
            }

            _context.Manufactures.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static void NormalizePaging(ref int page, ref int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 500) pageSize = 500;
        }
    }
}