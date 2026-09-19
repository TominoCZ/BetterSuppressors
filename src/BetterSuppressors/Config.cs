using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterSuppressors;

public enum AdjustmentMode
{
    Unchanged,
    Set,
    Add,
    ScalePenaltyAboveNeutral,
}

[JsonConverter(typeof(StatAdjustmentConverter))]
public sealed class StatAdjustment
{
    public AdjustmentMode Mode { get; set; } = AdjustmentMode.Unchanged;
    public double Value { get; set; }

    public double? Apply(double? current, double neutral = 0)
    {
        if (Mode == AdjustmentMode.Set)
        {
            return Value;
        }

        if (current is null)
        {
            return null;
        }

        return Mode switch
        {
            AdjustmentMode.Unchanged => current,
            AdjustmentMode.Add => current + Value,
            AdjustmentMode.ScalePenaltyAboveNeutral when current > neutral
                => neutral + ((current - neutral) * Value),
            AdjustmentMode.ScalePenaltyAboveNeutral => current,
            _ => current,
        };
    }
}

public sealed class StatAdjustmentConverter : JsonConverter<StatAdjustment>
{
    public override StatAdjustment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Version 1.0 used bare numbers. Treat them as absolute Set operations so an
        // existing tuned config keeps its old behavior after upgrading.
        if (reader.TokenType == JsonTokenType.Number)
        {
            return new StatAdjustment
            {
                Mode = AdjustmentMode.Set,
                Value = reader.GetDouble(),
            };
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("A stat adjustment must be a number or an object.");
        }

        var adjustment = new StatAdjustment();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Invalid stat adjustment property.");
            }

            var propertyName = reader.GetString();
            reader.Read();

            if (string.Equals(propertyName, nameof(StatAdjustment.Mode), StringComparison.OrdinalIgnoreCase))
            {
                var modeText = reader.GetString();
                if (!Enum.TryParse<AdjustmentMode>(modeText, true, out var mode))
                {
                    throw new JsonException($"Unknown adjustment mode '{modeText}'.");
                }

                adjustment.Mode = mode;
            }
            else if (string.Equals(propertyName, nameof(StatAdjustment.Value), StringComparison.OrdinalIgnoreCase))
            {
                adjustment.Value = reader.GetDouble();
            }
            else
            {
                reader.Skip();
            }
        }

        return adjustment;
    }

    public override void Write(Utf8JsonWriter writer, StatAdjustment value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(StatAdjustment.Mode), value.Mode.ToString());
        writer.WriteNumber(nameof(StatAdjustment.Value), value.Value);
        writer.WriteEndObject();
    }
}

public sealed class Config
{
    public StatAdjustment Ergonomics { get; set; } = new();
    public StatAdjustment DurabilityBurn { get; set; } = new()
    {
        Mode = AdjustmentMode.ScalePenaltyAboveNeutral,
        Value = 0.5,
    };
    public StatAdjustment HeatFactor { get; set; } = new();
    public StatAdjustment CoolFactor { get; set; } = new();
    public StatAdjustment Accuracy { get; set; } = new()
    {
        Mode = AdjustmentMode.Add,
        Value = 1.5,
    };
    public StatAdjustment Velocity { get; set; } = new()
    {
        Mode = AdjustmentMode.Add,
        Value = 2,
    };

    // Most suppressors are direct children of the vanilla suppressor category. Following
    // descendants also covers mods that introduce their own subcategories beneath it.
    public bool IncludeDescendants { get; set; } = true;
    public HashSet<string> AdditionalItemIds { get; set; } = [];
    public HashSet<string> ExcludedItemIds { get; set; } = [];

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
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
            source = $"built-in defaults (config.json unreadable: {ex.Message})";
            return new Config();
        }
    }
}
