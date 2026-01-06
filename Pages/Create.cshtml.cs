using InventoryManager.Data;
using InventoryManager.Models;
using InventoryManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryManager.Pages
{
    public class CreateModel : PageModel
    {
        private readonly InventoryDbContext _context;
        private readonly IStorageService _storageService;

        private readonly ILogger<CreateModel> _logger;

        public CreateModel(InventoryDbContext context, IStorageService storageService, ILogger<CreateModel> logger)
        {
            _context = context;
            _storageService = storageService;
            _logger = logger;
        }

        [BindProperty]
        public Equipment Equipment { get; set; } = default!;

        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        public IActionResult OnGet()
        {
            // Set defaults if needed
            Equipment = new Equipment { PurchaseDate = DateTime.Today, WarrantyExpirationDate = DateTime.Today.AddYears(1) };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create form model state is invalid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Error: {error.ErrorMessage}");
                }
                return Page();
            }

            if (UploadFile != null)
            {
                try 
                {
                    using var stream = UploadFile.OpenReadStream();
                    var fileName = $"documents/{Guid.NewGuid()}_{UploadFile.FileName}";
                    Equipment.DocumentUrl = await _storageService.UploadFileAsync(stream, fileName, UploadFile.ContentType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload file to S3. Continuing without document.");
                    // Optional: Add a UI warning
                    ModelState.AddModelError("", "Could not upload file (S3 config missing?), but equipment will be saved.");
                }
            }
            
            Equipment.Status = EquipmentStatus.InStock; // Default
            
            _context.Equipment.Add(Equipment);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
