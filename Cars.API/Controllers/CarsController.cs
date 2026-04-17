using Cars.API.Models;
using Cars.API.Settings;
using Cars.BLL.Dtos.Car;
using Cars.BLL.Dtos.Common;
using Cars.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cars.API.Controllers
{
    [ApiController]
    [Route("api/cars")]
    public class CarsController : ControllerBase
    {
        private readonly CarService _carService;
        private readonly string _carsPath;

        public CarsController(CarService carService, IWebHostEnvironment environment)
        {
            _carService = carService;

            string rootPath = environment.ContentRootPath;
            _carsPath = Path.Combine(rootPath, StaticFilesSettings.StorageDir, StaticFilesSettings.CarsDir);
            Directory.CreateDirectory(_carsPath);
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int page = 1,
            [FromQuery(Name = "page_size")] int pageSize = 100,
            [FromQuery] string? property = null,
            [FromQuery] string? value = null)
        {
            var dto = new GetCarsQueryDto
            {
                Page = page,
                PageSize = pageSize,
                Property = property,
                Value = value
            };

            var result = await _carService.GetAllAsync(dto);
            return Ok(new ApiResponseDto<PagedDataDto<CarItemDto>> { Data = result });
        }

        [HttpGet("{id:int}", Name = "GetCarById")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _carService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Car not found." });
            }

            return Ok(new ApiResponseDto<CarItemDto> { Data = result });
        }

        [HttpGet("by-price")]
        public async Task<IActionResult> GetByPriceAsync(
            [FromQuery] decimal minValue,
            [FromQuery] decimal maxValue,
            [FromQuery] int page = 1,
            [FromQuery(Name = "page_size")] int pageSize = 100)
        {
            var dto = new GetCarsByPriceQueryDto
            {
                MinValue = minValue,
                MaxValue = maxValue,
                Page = page,
                PageSize = pageSize
            };

            var result = await _carService.GetByPriceAsync(dto);
            return Ok(new ApiResponseDto<PagedDataDto<CarItemDto>> { Data = result });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] CreateCarDto dto)
        {
            var created = await _carService.CreateAsync(dto, _carsPath);
            return CreatedAtRoute("GetCarById", new { id = created.Id }, new ApiResponseDto<CarItemDto> { Data = created });
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] UpdateCarDto dto)
        {
            var updated = await _carService.UpdateAsync(id, dto, _carsPath);
            if (updated == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Car not found." });
            }

            return Ok(new ApiResponseDto<CarItemDto> { Data = updated });
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            bool deleted = await _carService.DeleteAsync(id, _carsPath);
            if (!deleted)
            {
                return NotFound(new ErrorResponseDto { Message = "Car not found." });
            }

            return NoContent();
        }
    }
}