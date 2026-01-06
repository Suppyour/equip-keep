using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManager.Models
{
    public enum EquipmentStatus
    {
        InStock = 0,
        Issued = 1,
        WrittenOff = 2
    }

    public class Employee
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        public ICollection<Equipment> CurrentEquipment { get; set; } = new List<Equipment>();
    }

    public class Equipment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Model { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }
        
        public DateTime WarrantyExpirationDate { get; set; }

        public string? DocumentUrl { get; set; } 

        public EquipmentStatus Status { get; set; }

        public int? CurrentOwnerId { get; set; }
        
        [ForeignKey(nameof(CurrentOwnerId))]
        public Employee? CurrentOwner { get; set; }

        public ICollection<HistoryRecord> History { get; set; } = new List<HistoryRecord>();
    }

    public class HistoryRecord
    {
        [Key]
        public int Id { get; set; }

        public int EquipmentId { get; set; }
        [ForeignKey(nameof(EquipmentId))]
        public Equipment Equipment { get; set; } = null!;

        public int? OldOwnerId { get; set; }
        [ForeignKey(nameof(OldOwnerId))]
        public Employee? OldOwner { get; set; }

        public int? NewOwnerId { get; set; }
        [ForeignKey(nameof(NewOwnerId))]
        public Employee? NewOwner { get; set; }

        public DateTime EventDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string? Comment { get; set; }
    }
}
