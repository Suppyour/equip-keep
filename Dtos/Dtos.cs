using Microsoft.AspNetCore.Http;
using InventoryManager.Models;

namespace InventoryManager.Dtos
{
    public class CreateEquipmentDto
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public DateTime WarrantyExpirationDate { get; set; }
        public IFormFile? File { get; set; }
    }

    public class TransferDto
    {
        public int? NewOwnerId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class ReportRequestDto
    {
        public EquipmentStatus? FilterStatus { get; set; }
    }
}
