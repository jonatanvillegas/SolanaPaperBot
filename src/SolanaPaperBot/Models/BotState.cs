namespace SolanaPaperBot.Models;

public sealed class BotState
{
    public decimal Balance { get; set; }
    public DateOnly CurrentDay { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public decimal DailyPnl { get; set; }
    public int DailyTrades { get; set; }
    public List<PaperTrade> Trades { get; set; } = [];
}
