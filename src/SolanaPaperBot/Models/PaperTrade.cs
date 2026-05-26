namespace SolanaPaperBot.Models;

public enum TradeSide
{
    Long,
    Short
}

public enum TradeStatus
{
    Open,
    Closed
}

public sealed class PaperTrade
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TradeSide Side { get; set; }
    public TradeStatus Status { get; set; } = TradeStatus.Open;
    public string Symbol { get; set; } = "SOLUSDT";
    public DateTime EntryTime { get; set; } = DateTime.UtcNow;
    public DateTime? ExitTime { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal? ExitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal PositionValue { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal StopLoss { get; set; }
    public decimal ProfitLoss { get; set; }
    public string EntryReason { get; set; } = string.Empty;
    public string? ExitReason { get; set; }
}
