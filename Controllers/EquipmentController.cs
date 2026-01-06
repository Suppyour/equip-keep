using InventoryManager.Data;
using InventoryManager.Dtos;
using InventoryManager.Models;
using InventoryManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IStorageService _storageService;

        public EquipmentController(InventoryDbContext context, IInventoryService inventoryService, IStorageService storageService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _storageService = storageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] EquipmentStatus? status)
        {
            var query = _context.Equipment.Include(e => e.CurrentOwner).AsQueryable();
            if (status.HasValue) query = query.Where(e => e.Status == status);
            return Ok(await query.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Equipment
                .Include(e => e.History).ThenInclude(h => h.OldOwner)
                .Include(e => e.History).ThenInclude(h => h.NewOwner)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateEquipmentDto dto)
        {
            string? docUrl = null;
            if (dto.File != null)
            {
                using var stream = dto.File.OpenReadStream();
                var fileName = $"documents/{Guid.NewGuid()}_{dto.File.FileName}";
                docUrl = await _storageService.UploadFileAsync(stream, fileName, dto.File.ContentType);
            }

            var equipment = new Equipment
            {
                SerialNumber = dto.SerialNumber,
                Model = dto.Model,
                PurchaseDate = dto.PurchaseDate,
                WarrantyExpirationDate = dto.WarrantyExpirationDate,
                DocumentUrl = docUrl,
                Status = EquipmentStatus.InStock
            };

            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = equipment.Id }, equipment);
        }

        [HttpPost("{id}/transfer")]
        public async Task<IActionResult> Transfer(int id, [FromBody] TransferDto dto)
        {
            await _inventoryService.TransferEquipmentAsync(id, dto.NewOwnerId, dto.Comment);
            return Ok();
        }

        [HttpPost("generate-report")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDto request)
        {
            var url = await _inventoryService.GenerateReportAsync(request.FilterStatus);
            return Ok(new { downloadUrl = url });
        }
    }
}
