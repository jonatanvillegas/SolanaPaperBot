using System.Globalization;
using System.Text.Json;
using SolanaPaperBot.Models;
using SolanaPaperBot.Options;
using Microsoft.Extensions.Options;

namespace SolanaPaperBot.Services;

public sealed class BinanceMarketService
{
    private readonly HttpClient _http;
    private readonly BotOptions _options;

    public BinanceMarketService(HttpClient http, IOptions<BotOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<List<Candle>> GetCandlesAsync(CancellationToken cancellationToken)
    {
        var url = $"/api/v3/klines?symbol={_options.Symbol}&interval={_options.Timeframe}&limit={_options.CandleLimit}";
        using var response = await _http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var candles = new List<Candle>();
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var arr = item.EnumerateArray().ToArray();
            var openTimeMs = arr[0].GetInt64();

            candles.Add(new Candle(
                DateTimeOffset.FromUnixTimeMilliseconds(openTimeMs).UtcDateTime,
                Parse(arr[1]),
                Parse(arr[2]),
                Parse(arr[3]),
                Parse(arr[4]),
                Parse(arr[5])
            ));
        }

        return candles;
    }

    private static decimal Parse(JsonElement value)
        => decimal.Parse(value.GetString()!, CultureInfo.InvariantCulture);
}
