namespace SolanaPaperBot.Models;

public enum SignalType
{
    None,
    Long,
    Short
}

public sealed record Signal(
    SignalType Type,
    string Reason,
    decimal Rsi,
    decimal EmaFast,
    decimal EmaSlow,
    decimal EmaTrend
);
