using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ClosedXML.Excel;
using InventoryManager.Data;
using InventoryManager.Dtos;
using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        string GetFileUrl(string fileName);
    }

    public class StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public StorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["YandexCloud:StorageBucket"] ?? "inventory-bucket";
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.Private, 
                ContentType = contentType
            };
            await fileTransferUtility.UploadAsync(uploadRequest);
            return fileName; 
        }

        public string GetFileUrl(string fileName)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            return _s3Client.GetPreSignedURL(request);
        }
    }

    public interface IInventoryService
    {
        Task TransferEquipmentAsync(int equipmentId, int? newOwnerId, string comment);
        Task<string> GenerateReportAsync(EquipmentStatus? filterStatus);
    }

    public class InventoryService : IInventoryService
    {
        private readonly InventoryDbContext _context;
        private readonly IStorageService _storageService;

        public InventoryService(InventoryDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task TransferEquipmentAsync(int equipmentId, int? newOwnerId, string comment)
        {
            var equipment = await _context.Equipment.FindAsync(equipmentId)
                           ?? throw new KeyNotFoundException("Equipment not found");

            var oldOwnerId = equipment.CurrentOwnerId;

            equipment.CurrentOwnerId = newOwnerId;
            equipment.Status = newOwnerId.HasValue ? EquipmentStatus.Issued : EquipmentStatus.InStock;

            var history = new HistoryRecord
            {
                EquipmentId = equipmentId,
                OldOwnerId = oldOwnerId,
                NewOwnerId = newOwnerId,
                EventDate = DateTime.UtcNow,
                Comment = comment
            };

            _context.History.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerateReportAsync(EquipmentStatus? filterStatus)
        {
            var query = _context.Equipment.Include(e => e.CurrentOwner).AsQueryable();

            if (filterStatus.HasValue)
            {
                query = query.Where(e => e.Status == filterStatus.Value);
            }

            var equipmentList = await query.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventory Report");

            worksheet.Cell(1, 1).Value = "Serial Number";
            worksheet.Cell(1, 2).Value = "Model";
            worksheet.Cell(1, 3).Value = "Status";
            worksheet.Cell(1, 4).Value = "Owner";
            worksheet.Cell(1, 5).Value = "Purchase Date";
            worksheet.Cell(1, 6).Value = "Warranty Expiration";

            for (int i = 0; i < equipmentList.Count; i++)
            {
                var item = equipmentList[i];
                var row = i + 2;
                worksheet.Cell(row, 1).Value = item.SerialNumber;
                worksheet.Cell(row, 2).Value = item.Model;
                worksheet.Cell(row, 3).Value = item.Status.ToString();
                worksheet.Cell(row, 4).Value = item.CurrentOwner?.FullName ?? "N/A";
                worksheet.Cell(row, 5).Value = item.PurchaseDate;
                worksheet.Cell(row, 6).Value = item.WarrantyExpirationDate;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"reports/inventory_report_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            await _storageService.UploadFileAsync(stream, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            return _storageService.GetFileUrl(fileName);
        }
    }
}
