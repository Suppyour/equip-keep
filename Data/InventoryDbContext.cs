using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<HistoryRecord> History { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.SerialNumber)
                .IsUnique();

            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.CurrentOwner)
                .WithMany(em => em.CurrentEquipment)
                .HasForeignKey(e => e.CurrentOwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<HistoryRecord>()
                .HasOne(h => h.Equipment)
                .WithMany(e => e.History)
                .HasForeignKey(h => h.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HistoryRecord>()
                .HasOne(h => h.OldOwner)
                .WithMany()
                .HasForeignKey(h => h.OldOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HistoryRecord>()
                .HasOne(h => h.NewOwner)
                .WithMany()
                .HasForeignKey(h => h.NewOwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
