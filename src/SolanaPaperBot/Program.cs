using SolanaPaperBot.Options;
using SolanaPaperBot.Services;
using SolanaPaperBot.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Bot"));

builder.Services.AddHttpClient<BinanceMarketService>(client =>
{
    client.BaseAddress = new Uri("https://api.binance.com");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddSingleton<TradeStorage>();
builder.Services.AddSingleton<StrategyService>();
builder.Services.AddSingleton<PaperTraderService>();
builder.Services.AddHostedService<TradingWorker>();

var app = builder.Build();

app.MapGet("/", () => "Solana Paper Bot running");

app.MapGet("/status", (PaperTraderService trader) =>
{
    return Results.Ok(trader.GetStatus());
});

app.Run();