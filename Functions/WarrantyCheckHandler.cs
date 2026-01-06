using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryManager.Functions
{
    public class WarrantyCheckHandler
    {
        public async Task<string> FunctionHandler(string input, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<WarrantyCheckHandler>>();
            var dbContext = serviceProvider.GetRequiredService<InventoryDbContext>();

            logger.LogInformation("Starting Warranty Check...");

            var today = DateTime.UtcNow.Date;
            var thirtyDaysFromNow = today.AddDays(30);

            var expiringItems = await dbContext.Equipment
                .Where(e => e.Status != Models.EquipmentStatus.WrittenOff 
                            && e.WarrantyExpirationDate >= today 
                            && e.WarrantyExpirationDate <= thirtyDaysFromNow)
                .ToListAsync();

            foreach (var item in expiringItems)
            {
                logger.LogWarning($"Warranty expiring for {item.Model} (S/N: {item.SerialNumber}) on {item.WarrantyExpirationDate:d}");
            }

            logger.LogInformation($"Check complete. Found {expiringItems.Count} expiring items.");
            return $"Checked {expiringItems.Count} items.";
        }
    }
}
