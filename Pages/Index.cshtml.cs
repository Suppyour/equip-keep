using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Pages
{
    public class IndexModel : PageModel
    {
        private readonly InventoryDbContext _context;

        public IndexModel(InventoryDbContext context)
        {
            _context = context;
        }

        public IList<Equipment> Equipment { get;set; } = default!;

        [BindProperty(SupportsGet = true)]
        public EquipmentStatus? FilterStatus { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Equipment
                .Include(e => e.CurrentOwner)
                .AsNoTracking();

            if (FilterStatus.HasValue)
            {
                query = query.Where(e => e.Status == FilterStatus.Value);
            }

            Equipment = await query.ToListAsync();
        }
    }
}
