namespace RMitra.Domain.Settlement;

public class LedgerLine
{
    public long Id { get; set; }
    public string PartyType { get; set; } = string.Empty;
    public Guid PartyGuid { get; set; }
    public Guid OrderGuid { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SettlementBatch
{
    public Guid Id { get; set; }
    public string SettlementId { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty;
    public Guid PartyGuid { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? InvoiceUrl { get; set; }
    public string? Utr { get; set; }
    public DateTime CreatedAt { get; set; }
}
