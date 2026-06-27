using Cars.API.Models;
using Cars.BLL.Dtos.Common;
using Cars.BLL.Dtos.Manufacture;
using Cars.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cars.API.Controllers
{
    [ApiController]
    [Route("api/manufactures")]
    public class ManufacturesController : ControllerBase
    {
        private readonly ManufactureService _manufactureService;

        public ManufacturesController(ManufactureService manufactureService)
        {
            _manufactureService = manufactureService;
        }

        // GET без [Authorize] — список виробників публічний, бо фронт показує їх в дропдауні при створенні авто
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] int page = 1, [FromQuery(Name = "page_size")] int pageSize = 100)
        {
            var result = await _manufactureService.GetAllAsync(page, pageSize);
            return Ok(new ApiResponseDto<PagedDataDto<ManufactureItemDto>> { Data = result });
        }

        [HttpGet("{id:int}", Name = "GetManufactureById")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _manufactureService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Manufacture not found." });
            }

            return Ok(new ApiResponseDto<ManufactureItemDto> { Data = result });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateManufactureDto dto)
        {
            var created = await _manufactureService.CreateAsync(dto);
            // 201 + Location header, аналогічно CarsController
            return CreatedAtRoute("GetManufactureById", new { id = created.Id }, new ApiResponseDto<ManufactureItemDto> { Data = created });
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateManufactureDto dto)
        {
            var updated = await _manufactureService.UpdateAsync(id, dto);
            if (updated == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Manufacture not found." });
            }

            return Ok(new ApiResponseDto<ManufactureItemDto> { Data = updated });
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            // false може бути і через неіснуючий id, і через каскадне блокування —
            // BLL кидає ArgumentException якщо виробник має пов'язані авто, а middleware перехопить це в 400
            bool deleted = await _manufactureService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new ErrorResponseDto { Message = "Manufacture not found." });
            }

            return NoContent();
        }
    }
}