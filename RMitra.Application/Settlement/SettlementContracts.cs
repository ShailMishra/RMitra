namespace RMitra.Application.Settlement;

public class LedgerDto
{
    public string PartyType { get; set; } = string.Empty;
    public decimal WeekTotal { get; set; }
    public IReadOnlyList<LedgerLineDto> Lines { get; set; } = [];
}

public class LedgerLineDto
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SettlementDto
{
    public string SettlementId { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? InvoiceUrl { get; set; }
    public string? Utr { get; set; }
}

public class SettlementConfigDto
{
    public decimal CommissionPercent { get; set; }
    public decimal GstPercent { get; set; }
    public decimal RiderSharePercent { get; set; }
    public decimal ExtraAfter3Km { get; set; }
    public decimal ExtraAfter6Km { get; set; }
}

public interface ISettlementService
{
    Task CreditOnDeliveredAsync(Guid orderGuid, CancellationToken cancellationToken = default);
    Task<LedgerDto> GetKitchenLedgerAsync(string kitchenId, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<LedgerDto> GetRiderLedgerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SettlementDto>> RunWeeklyAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SettlementDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<SettlementDto> GetAsync(string settlementId, CancellationToken cancellationToken = default);
    SettlementConfigDto GetConfig();
}
