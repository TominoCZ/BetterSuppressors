using System.Reflection;
using System.Text.Json;

namespace BetterSuppressors;

public sealed class Config
{
    public double Ergonomics { get; set; } = 0;
    public double DurabilityBurn { get; set; } = 1.0;

    // Both heat fields are penalties above 1.0 and improvements below it. CoolFactor
    // especially does not read that way, but the vanilla heavy barrels sit at 0.82/0.86
    // and they are the best heat sinks in the game.
    public double HeatFactor { get; set; } = 0.90;
    public double CoolFactor { get; set; } = 0.92;

    public double Accuracy { get; set; } = 6;
    public double Velocity { get; set; } = 6;

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static Config Load(out string source)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.Combine(string.IsNullOrEmpty(dir) ? "." : dir, "config.json");

        if (!File.Exists(path))
        {
            source = "built-in defaults (no config.json)";
            return new Config();
        }

        try
        {
            var loaded = JsonSerializer.Deserialize<Config>(File.ReadAllText(path), ReadOptions);
            if (loaded is null)
            {
                source = "built-in defaults (config.json was empty)";
                return new Config();
            }

            source = "config.json";
            return loaded;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // A typo in the config shouldn't take the server down mid-load.
            source = $"built-in defaults (config.json unreadable: {ex.Message})";
            return new Config();
        }
    }
}
