using SolanaPaperBot.Models;
using SolanaPaperBot.Options;
using SolanaPaperBot.Storage;
using Microsoft.Extensions.Options;

namespace SolanaPaperBot.Services;

public sealed class PaperTraderService
{
    private readonly BotOptions _options;
    private readonly TradeStorage _storage;
    private readonly ILogger<PaperTraderService> _logger;

    public PaperTraderService(IOptions<BotOptions> options, TradeStorage storage, ILogger<PaperTraderService> logger)
    {
        _options = options.Value;
        _storage = storage;
        _logger = logger;
    }

    public async Task ProcessAsync(decimal price, Signal signal, CancellationToken cancellationToken)
    {
        var state = await _storage.LoadAsync(cancellationToken);
        ResetDailyStatsIfNeeded(state);

        var openTrade = state.Trades.FirstOrDefault(x => x.Status == TradeStatus.Open);
        if (openTrade is not null)
        {
            CloseIfNeeded(state, openTrade, price);
            await _storage.SaveAsync(state, cancellationToken);
            return;
        }

        if (state.DailyPnl >= _options.DailyProfitTarget)
        {
            _logger.LogInformation("Meta diaria alcanzada: {DailyPnl:C}. No se abrirán más operaciones hoy.", state.DailyPnl);
            await _storage.SaveAsync(state, cancellationToken);
            return;
        }

        if (state.DailyPnl <= -_options.DailyMaxLoss)
        {
            _logger.LogWarning("Pérdida máxima diaria alcanzada: {DailyPnl:C}. Bot pausado hasta mañana.", state.DailyPnl);
            await _storage.SaveAsync(state, cancellationToken);
            return;
        }

        if (state.DailyTrades >= _options.MaxTradesPerDay)
        {
            _logger.LogInformation("Máximo de trades diarios alcanzado: {Count}.", state.DailyTrades);
            await _storage.SaveAsync(state, cancellationToken);
            return;
        }

        if (signal.Type == SignalType.Long && _options.AllowLong)
            OpenTrade(state, TradeSide.Long, price, signal.Reason);
        else if (signal.Type == SignalType.Short && _options.AllowShort)
            OpenTrade(state, TradeSide.Short, price, signal.Reason);

        await _storage.SaveAsync(state, cancellationToken);
    }

    public async Task<object> GetStatusAsync(CancellationToken cancellationToken)
    {
        var state = await _storage.LoadAsync(cancellationToken);

        var openTrade = state.Trades.FirstOrDefault(x => x.Status == TradeStatus.Open);

        var wins = state.Trades.Count(x => x.Status == TradeStatus.Closed && x.ProfitLoss > 0);
        var losses = state.Trades.Count(x => x.Status == TradeStatus.Closed && x.ProfitLoss < 0);

        return new
        {
            state.Balance,
            DailyProfit = state.DailyPnl,
            state.DailyTrades,
            Wins = wins,
            Losses = losses,
            OpenTrade = openTrade,
            Trades = state.Trades
        };
    }
    private void OpenTrade(BotState state, TradeSide side, decimal price, string reason)
    {
        var positionValue = state.Balance * _options.PositionSizePercent;
        var quantity = positionValue / price;

        var takeProfit = side == TradeSide.Long
            ? price * (1 + _options.TakeProfitPercent)
            : price * (1 - _options.TakeProfitPercent);

        var stopLoss = side == TradeSide.Long
            ? price * (1 - _options.StopLossPercent)
            : price * (1 + _options.StopLossPercent);

        var trade = new PaperTrade
        {
            Side = side,
            Symbol = _options.Symbol,
            EntryPrice = price,
            Quantity = quantity,
            PositionValue = positionValue,
            TakeProfit = takeProfit,
            StopLoss = stopLoss,
            EntryReason = reason
        };

        state.Trades.Add(trade);
        state.DailyTrades++;

        _logger.LogInformation("ABRIENDO {Side} | Precio: {Price} | TP: {TP} | SL: {SL} | Monto: {Value:C}",
            side, price, takeProfit, stopLoss, positionValue);
    }

    private void CloseIfNeeded(BotState state, PaperTrade trade, decimal price)
    {
        var hitTakeProfit = trade.Side == TradeSide.Long
            ? price >= trade.TakeProfit
            : price <= trade.TakeProfit;

        var hitStopLoss = trade.Side == TradeSide.Long
            ? price <= trade.StopLoss
            : price >= trade.StopLoss;

        if (!hitTakeProfit && !hitStopLoss)
        {
            _logger.LogInformation("Trade abierto {Side} | Entrada: {Entry} | Precio actual: {Price} | TP: {TP} | SL: {SL}",
                trade.Side, trade.EntryPrice, price, trade.TakeProfit, trade.StopLoss);
            return;
        }

        var pnl = trade.Side == TradeSide.Long
            ? (price - trade.EntryPrice) * trade.Quantity
            : (trade.EntryPrice - price) * trade.Quantity;

        trade.Status = TradeStatus.Closed;
        trade.ExitTime = DateTime.UtcNow;
        trade.ExitPrice = price;
        trade.ProfitLoss = pnl;
        trade.ExitReason = hitTakeProfit ? "TAKE_PROFIT" : "STOP_LOSS";

        state.Balance += pnl;
        state.DailyPnl += pnl;

        _logger.LogInformation("CERRANDO {Side} por {Reason} | Salida: {Exit} | PnL: {Pnl:C} | Balance: {Balance:C}",
            trade.Side, trade.ExitReason, price, pnl, state.Balance);
    }

    private static void ResetDailyStatsIfNeeded(BotState state)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (state.CurrentDay == today) return;

        state.CurrentDay = today;
        state.DailyPnl = 0;
        state.DailyTrades = 0;
    }
}
