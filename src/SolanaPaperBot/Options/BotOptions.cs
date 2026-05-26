namespace SolanaPaperBot.Options;

public sealed class BotOptions
{
    public string Symbol { get; set; } = "SOLUSDT";
    public string Timeframe { get; set; } = "15m";
    public decimal InitialBalance { get; set; } = 100m;
    public decimal DailyProfitTarget { get; set; } = 2m;
    public decimal DailyMaxLoss { get; set; } = 3m;
    public decimal RiskPerTrade { get; set; } = 0.01m;
    public decimal TakeProfitPercent { get; set; } = 0.006m;
    public decimal StopLossPercent { get; set; } = 0.004m;
    public decimal PositionSizePercent { get; set; } = 0.15m;
    public int PollSeconds { get; set; } = 60;
    public int CandleLimit { get; set; } = 120;
    public int MaxTradesPerDay { get; set; } = 5;
    public bool AllowLong { get; set; } = true;
    public bool AllowShort { get; set; } = true;
}
