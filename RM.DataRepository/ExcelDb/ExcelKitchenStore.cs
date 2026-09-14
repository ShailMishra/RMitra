using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;

namespace RM.DataRepository.ExcelDb
{
    public interface IExcelKitchenStore
    {
        bool CanConnect();
        string FilePath { get; }
        void InsertKitchen(ExcelKitchenRow kitchen);
        ExcelKitchenRow? GetKitchen(string kitchenId);
        List<ExcelKitchenRow> GetKitchensByStatuses(IReadOnlyCollection<string> statuses);
        void UpdateKitchen(ExcelKitchenRow kitchen);
        void InsertStatusHistory(string kitchenId, string status, string? remarks, DateTime createdAtUtc);
    }

    public sealed class ExcelKitchenRow
    {
        public string KitchenId { get; set; } = string.Empty;
        public string KitchenName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string KitchenType { get; set; } = string.Empty;
        public string CuisineTypes { get; set; } = string.Empty;
        public string OperatingHours { get; set; } = string.Empty;
        public string BankDetails { get; set; } = string.Empty;
        public string PanCard { get; set; } = string.Empty;
        public string KitchenPhoto { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string VerificationToken { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
        public string AdditionalDocumentsRequired { get; set; } = string.Empty;
        public string AdminRemarks { get; set; } = string.Empty;
        public string ResubmittedDocuments { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public sealed class ExcelKitchenStore : IExcelKitchenStore
    {
        private const string KitchensSheet = "RM_Kitchens";
        private const string HistorySheet = "RM_KitchenStatusHistory";
        private readonly object _sync = new();
        private static readonly string[] KitchenHeaders =
        {
            "KitchenId", "KitchenName", "OwnerName", "MobileNumber", "Email",
            "AddressLine1", "AddressLine2", "City", "State", "Pincode",
            "KitchenType", "CuisineTypes", "OperatingHours", "BankDetails",
            "PanCard", "KitchenPhoto", "Status", "VerificationToken",
            "RejectionReason", "AdditionalDocumentsRequired", "AdminRemarks",
            "ResubmittedDocuments", "CreatedAt", "UpdatedAt", "SubmittedAt"
        };

        public ExcelKitchenStore(IConfiguration configuration)
        {
            var relativePath = configuration["ExcelDb:FilePath"] ?? "App_Data/RasoiMitra.xlsx";
            FilePath = Path.IsPathRooted(relativePath)
                ? relativePath
                : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
        }

        public string FilePath { get; }

        public bool CanConnect()
        {
            lock (_sync)
            {
                EnsureWorkbook();
                using var workbook = new XLWorkbook(FilePath);
                return workbook.TryGetWorksheet(KitchensSheet, out _)
                    && workbook.TryGetWorksheet(HistorySheet, out _);
            }
        }

        public void InsertKitchen(ExcelKitchenRow kitchen)
        {
            lock (_sync)
            {
                using var workbook = OpenWorkbook();
                var sheet = workbook.Worksheet(KitchensSheet);
                WriteKitchen(sheet.Row(NextDataRow(sheet)), kitchen);
                workbook.Save();
            }
        }

        public ExcelKitchenRow? GetKitchen(string kitchenId)
        {
            lock (_sync)
            {
                using var workbook = OpenWorkbook();
                var sheet = workbook.Worksheet(KitchensSheet);
                var row = FindKitchenRow(sheet, kitchenId);
                return row == null ? null : ReadKitchen(row);
            }
        }

        public List<ExcelKitchenRow> GetKitchensByStatuses(IReadOnlyCollection<string> statuses)
        {
            lock (_sync)
            {
                using var workbook = OpenWorkbook();
                var sheet = workbook.Worksheet(KitchensSheet);
                var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
                var result = new List<ExcelKitchenRow>();

                for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
                {
                    var kitchen = ReadKitchen(sheet.Row(rowNumber));
                    if (statuses.Contains(kitchen.Status, StringComparer.OrdinalIgnoreCase))
                        result.Add(kitchen);
                }

                return result
                    .OrderBy(k => k.SubmittedAt ?? DateTime.MaxValue)
                    .ToList();
            }
        }

        public void UpdateKitchen(ExcelKitchenRow kitchen)
        {
            lock (_sync)
            {
                using var workbook = OpenWorkbook();
                var sheet = workbook.Worksheet(KitchensSheet);
                var row = FindKitchenRow(sheet, kitchen.KitchenId)
                    ?? throw new InvalidOperationException($"Kitchen '{kitchen.KitchenId}' was not found in Excel DB.");
                WriteKitchen(row, kitchen);
                workbook.Save();
            }
        }

        public void InsertStatusHistory(string kitchenId, string status, string? remarks, DateTime createdAtUtc)
        {
            lock (_sync)
            {
                using var workbook = OpenWorkbook();
                var sheet = workbook.Worksheet(HistorySheet);
                var rowNumber = NextDataRow(sheet);
                var nextId = rowNumber - 1;
                var row = sheet.Row(rowNumber);
                row.Cell(1).Value = nextId;
                row.Cell(2).Value = kitchenId;
                row.Cell(3).Value = status;
                row.Cell(4).Value = remarks ?? string.Empty;
                row.Cell(5).Value = createdAtUtc.ToString("o");
                workbook.Save();
            }
        }

        private XLWorkbook OpenWorkbook()
        {
            EnsureWorkbook();
            return new XLWorkbook(FilePath);
        }

        private void EnsureWorkbook()
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            if (File.Exists(FilePath))
                return;

            using var workbook = new XLWorkbook();
            var kitchens = workbook.AddWorksheet(KitchensSheet);
            for (var i = 0; i < KitchenHeaders.Length; i++)
                kitchens.Cell(1, i + 1).Value = KitchenHeaders[i];
            kitchens.SheetView.FreezeRows(1);
            kitchens.Row(1).Style.Font.Bold = true;

            var history = workbook.AddWorksheet(HistorySheet);
            history.Cell(1, 1).Value = "Id";
            history.Cell(1, 2).Value = "KitchenId";
            history.Cell(1, 3).Value = "Status";
            history.Cell(1, 4).Value = "Remarks";
            history.Cell(1, 5).Value = "CreatedAt";
            history.SheetView.FreezeRows(1);
            history.Row(1).Style.Font.Bold = true;

            workbook.SaveAs(FilePath);
        }

        private static int NextDataRow(IXLWorksheet sheet)
        {
            return (sheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
        }

        private static IXLRow? FindKitchenRow(IXLWorksheet sheet, string kitchenId)
        {
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
            {
                var row = sheet.Row(rowNumber);
                if (string.Equals(row.Cell(1).GetString(), kitchenId, StringComparison.OrdinalIgnoreCase))
                    return row;
            }

            return null;
        }

        private static ExcelKitchenRow ReadKitchen(IXLRow row)
        {
            return new ExcelKitchenRow
            {
                KitchenId = row.Cell(1).GetString(),
                KitchenName = row.Cell(2).GetString(),
                OwnerName = row.Cell(3).GetString(),
                MobileNumber = row.Cell(4).GetString(),
                Email = row.Cell(5).GetString(),
                AddressLine1 = row.Cell(6).GetString(),
                AddressLine2 = row.Cell(7).GetString(),
                City = row.Cell(8).GetString(),
                State = row.Cell(9).GetString(),
                Pincode = row.Cell(10).GetString(),
                KitchenType = row.Cell(11).GetString(),
                CuisineTypes = row.Cell(12).GetString(),
                OperatingHours = row.Cell(13).GetString(),
                BankDetails = row.Cell(14).GetString(),
                PanCard = row.Cell(15).GetString(),
                KitchenPhoto = row.Cell(16).GetString(),
                Status = row.Cell(17).GetString(),
                VerificationToken = row.Cell(18).GetString(),
                RejectionReason = row.Cell(19).GetString(),
                AdditionalDocumentsRequired = row.Cell(20).GetString(),
                AdminRemarks = row.Cell(21).GetString(),
                ResubmittedDocuments = row.Cell(22).GetString(),
                CreatedAt = ReadDate(row.Cell(23)),
                UpdatedAt = ReadDate(row.Cell(24)),
                SubmittedAt = ReadNullableDate(row.Cell(25))
            };
        }

        private static void WriteKitchen(IXLRow row, ExcelKitchenRow kitchen)
        {
            row.Cell(1).Value = kitchen.KitchenId;
            row.Cell(2).Value = kitchen.KitchenName;
            row.Cell(3).Value = kitchen.OwnerName;
            row.Cell(4).Value = kitchen.MobileNumber;
            row.Cell(5).Value = kitchen.Email;
            row.Cell(6).Value = kitchen.AddressLine1;
            row.Cell(7).Value = kitchen.AddressLine2;
            row.Cell(8).Value = kitchen.City;
            row.Cell(9).Value = kitchen.State;
            row.Cell(10).Value = kitchen.Pincode;
            row.Cell(11).Value = kitchen.KitchenType;
            row.Cell(12).Value = kitchen.CuisineTypes;
            row.Cell(13).Value = kitchen.OperatingHours;
            row.Cell(14).Value = kitchen.BankDetails;
            row.Cell(15).Value = kitchen.PanCard;
            row.Cell(16).Value = kitchen.KitchenPhoto;
            row.Cell(17).Value = kitchen.Status;
            row.Cell(18).Value = kitchen.VerificationToken;
            row.Cell(19).Value = kitchen.RejectionReason;
            row.Cell(20).Value = kitchen.AdditionalDocumentsRequired;
            row.Cell(21).Value = kitchen.AdminRemarks;
            row.Cell(22).Value = kitchen.ResubmittedDocuments;
            row.Cell(23).Value = kitchen.CreatedAt.ToString("o");
            row.Cell(24).Value = kitchen.UpdatedAt.ToString("o");
            row.Cell(25).Value = kitchen.SubmittedAt?.ToString("o") ?? string.Empty;
        }

        private static DateTime ReadDate(IXLCell cell)
        {
            var text = cell.GetString();
            return DateTime.TryParse(text, out var value) ? value : DateTime.MinValue;
        }

        private static DateTime? ReadNullableDate(IXLCell cell)
        {
            var text = cell.GetString();
            return DateTime.TryParse(text, out var value) ? value : null;
        }
    }
}
