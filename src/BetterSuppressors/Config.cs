using System.Reflection;
using System.Text.Json;

namespace BetterSuppressors;

public sealed class Config
{
    public double Ergonomics { get; set; } = 0;
    public double DurabilityBurn { get; set; } = 1.0;

    // These two run in opposite directions. HeatFactor is how fast the weapon heats, so
    // lower is better. CoolFactor is how fast it sheds that heat, so higher is better --
    // the best part in the game for it is a carbon fibre handguard at 1.25.
    public double HeatFactor { get; set; } = 0.90;
    public double CoolFactor { get; set; } = 1.25;

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
