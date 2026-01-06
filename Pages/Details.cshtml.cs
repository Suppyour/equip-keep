using InventoryManager.Data;
using InventoryManager.Models;
using InventoryManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly InventoryDbContext _context;
        private readonly IInventoryService _inventoryService;

        public DetailsModel(InventoryDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public Equipment Equipment { get; set; } = default!;
        
        [BindProperty]
        public int? NewOwnerId { get; set; }
        
        [BindProperty]
        public string TransferComment { get; set; } = string.Empty;

        public SelectList EmployeeList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var equipment = await _context.Equipment
                .Include(e => e.CurrentOwner)
                .Include(e => e.History).ThenInclude(h => h.OldOwner)
                .Include(e => e.History).ThenInclude(h => h.NewOwner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (equipment == null) return NotFound();
            
            Equipment = equipment;

            var employees = await _context.Employees.OrderBy(e => e.FullName).ToListAsync();
            EmployeeList = new SelectList(employees, "Id", "FullName");

            return Page();
        }

        public async Task<IActionResult> OnPostTransferAsync(int id)
        {
             await _inventoryService.TransferEquipmentAsync(id, NewOwnerId, TransferComment);
             return RedirectToPage("./Details", new { id });
        }
    }
}
