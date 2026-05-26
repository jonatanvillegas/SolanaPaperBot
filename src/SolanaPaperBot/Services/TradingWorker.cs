using SolanaPaperBot.Models;

namespace SolanaPaperBot.Services;

public sealed class TradingWorker : BackgroundService
{
    private readonly BinanceMarketService _market;
    private readonly StrategyService _strategy;
    private readonly PaperTraderService _paperTrader;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TradingWorker> _logger;

    public TradingWorker(
        BinanceMarketService market,
        StrategyService strategy,
        PaperTraderService paperTrader,
        IConfiguration configuration,
        ILogger<TradingWorker> logger)
    {
        _market = market;
        _strategy = strategy;
        _paperTrader = paperTrader;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollSeconds = _configuration.GetValue("Bot:PollSeconds", 60);

        _logger.LogInformation("Bot PAPER TRADING iniciado. No ejecuta órdenes reales.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var candles = await _market.GetCandlesAsync(stoppingToken);
                var lastPrice = candles[^1].Close;
                var signal = _strategy.Analyze(candles);

                _logger.LogInformation(
                    "Precio: {Price} | Señal: {Signal} | RSI: {Rsi:N2} | EMA9: {EmaFast:N2} | EMA21: {EmaSlow:N2} | EMA99: {EmaTrend:N2} | {Reason}",
                    lastPrice,
                    signal.Type,
                    signal.Rsi,
                    signal.EmaFast,
                    signal.EmaSlow,
                    signal.EmaTrend,
                    signal.Reason
                );

                if (signal.Type != SignalType.None)
                    await _paperTrader.ProcessAsync(lastPrice, signal, stoppingToken);
                else
                    await _paperTrader.ProcessAsync(lastPrice, signal, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando ciclo del bot");
            }

            await Task.Delay(TimeSpan.FromSeconds(pollSeconds), stoppingToken);
        }
    }
}
