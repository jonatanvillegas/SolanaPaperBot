using SolanaPaperBot.Models;

namespace SolanaPaperBot.Services;

public sealed class StrategyService
{
    public Signal Analyze(IReadOnlyList<Candle> candles)
    {
        if (candles.Count < 100)
            return new Signal(SignalType.None, "No hay suficientes velas", 0, 0, 0, 0);

        var closes = candles.Select(x => x.Close).ToList();
        var rsi = CalculateRsi(closes, 14);
        var emaFast = CalculateEma(closes, 9);
        var emaSlow = CalculateEma(closes, 21);
        var emaTrend = CalculateEma(closes, 99);
        var price = closes[^1];

        if (price > emaTrend && emaFast > emaSlow && rsi > 52 && rsi < 65)
        {
            return new Signal(
                SignalType.Long,
                "LONG: precio sobre EMA99, EMA9 sobre EMA21 y RSI sano",
                rsi,
                emaFast,
                emaSlow,
                emaTrend
            );
        }

        if (price < emaTrend && emaFast < emaSlow && rsi > 38 && rsi < 50)
        {
            return new Signal(
                SignalType.Short,
                "SHORT: precio bajo EMA99, EMA9 bajo EMA21 y RSI débil",
                rsi,
                emaFast,
                emaSlow,
                emaTrend
            );
        }

        return new Signal(
            SignalType.None,
            "Sin señal clara",
            rsi,
            emaFast,
            emaSlow,
            emaTrend
        );
    }

    private static decimal CalculateEma(IReadOnlyList<decimal> values, int period)
    {
        var multiplier = 2m / (period + 1);
        var ema = values.Take(period).Average();

        for (var i = period; i < values.Count; i++)
            ema = ((values[i] - ema) * multiplier) + ema;

        return ema;
    }

    private static decimal CalculateRsi(IReadOnlyList<decimal> values, int period)
    {
        decimal gain = 0;
        decimal loss = 0;

        for (var i = values.Count - period; i < values.Count; i++)
        {
            var change = values[i] - values[i - 1];
            if (change >= 0) gain += change;
            else loss -= change;
        }

        if (loss == 0) return 100;
        var rs = gain / loss;
        return 100 - (100 / (1 + rs));
    }
}
