using System.Text.Json;
using SolanaPaperBot.Models;
using SolanaPaperBot.Options;
using Microsoft.Extensions.Options;

namespace SolanaPaperBot.Storage;

public sealed class TradeStorage
{
    private readonly string _filePath = "/app/data/state.json";
    private readonly BotOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public TradeStorage(IOptions<BotOptions> options)
    {
        _options = options.Value;
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    public async Task<BotState> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new BotState
            {
                Balance = _options.InitialBalance,
                CurrentDay = DateOnly.FromDateTime(DateTime.UtcNow)
            };
        }

        await using var stream = File.OpenRead(_filePath);
        var state = await JsonSerializer.DeserializeAsync<BotState>(stream, _jsonOptions, cancellationToken);
        return state ?? new BotState { Balance = _options.InitialBalance };
    }

    public async Task SaveAsync(BotState state, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, state, _jsonOptions, cancellationToken);
    }
}
