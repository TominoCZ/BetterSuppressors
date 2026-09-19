using BetterSuppressors;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using System.Text.Json;

// dotnet run --project tests/BetterSuppressors.SelfCheck

var flashHider = new MongoId("550aa4bf4bdc2dd6348b456b");
var modSuppressorCategory = new MongoId("aaaaaaaaaaaaaaaaaaaaaaaa");
var explicitSuppressorId = new MongoId("bbbbbbbbbbbbbbbbbbbbbbbb");
var excludedSuppressorId = new MongoId("cccccccccccccccccccccccc");

static TemplateItem Item(MongoId id, MongoId parent, string type, TemplateItemProperties? props) => new()
{
    Id = id,
    Name = "test",
    Parent = parent,
    Type = type,
    Properties = props,
};

static TemplateItemProperties Stock() => new()
{
    Ergonomics = -16,
    DurabilityBurnModificator = 1.6,
    HeatFactor = 1.14,
    CoolFactor = 1.03,
    Accuracy = -1,
    Velocity = 0.7,
    Recoil = -6,
    Loudness = -20,
};

var suppressorA = Item(new MongoId(), SuppressorPatch.SilencerParent, "Item", Stock());
var suppressorB = Item(excludedSuppressorId, SuppressorPatch.SilencerParent, "Item", Stock());
var categoryNode = Item(modSuppressorCategory, SuppressorPatch.SilencerParent, "Node", Stock());
var nestedModSuppressor = Item(new MongoId(), modSuppressorCategory, "Item", Stock());
var explicitModSuppressor = Item(explicitSuppressorId, flashHider, "Item", Stock());
var unrelatedMuzzle = Item(new MongoId(), flashHider, "Item", Stock());
var brokenEntry = Item(new MongoId(), SuppressorPatch.SilencerParent, "Item", null);

var db = new[]
{
    suppressorA,
    suppressorB,
    categoryNode,
    nestedModSuppressor,
    explicitModSuppressor,
    unrelatedMuzzle,
    brokenEntry,
};
var config = new Config
{
    AdditionalItemIds = [explicitSuppressorId.ToString()],
    ExcludedItemIds = [excludedSuppressorId.ToString()],
};

var patchedIds = SuppressorPatch.Apply(db, config);

var failures = new List<string>();
void Check(bool ok, string what)
{
    if (!ok) failures.Add(what);
}

Check(patchedIds.Count == 3, $"expected 3 patched suppressors, got {patchedIds.Count}");
Check(patchedIds.Contains(suppressorA.Id), "direct suppressor was skipped");
Check(patchedIds.Contains(nestedModSuppressor.Id), "nested mod suppressor was skipped");
Check(patchedIds.Contains(explicitModSuppressor.Id), "explicit mod suppressor was skipped");

foreach (var (name, item) in new[]
{
    ("direct", suppressorA),
    ("nested", nestedModSuppressor),
    ("explicit", explicitModSuppressor),
})
{
    var props = item.Properties!;
    Check(props.Ergonomics == -16, $"{name}: ergonomics changed to {props.Ergonomics}");
    Check(Math.Abs(props.DurabilityBurnModificator!.Value - 1.3) < 0.0001,
        $"{name}: durability burn {props.DurabilityBurnModificator}");
    Check(props.HeatFactor == 1.14, $"{name}: heat changed to {props.HeatFactor}");
    Check(props.CoolFactor == 1.03, $"{name}: cooling changed to {props.CoolFactor}");
    Check(Math.Abs(props.Accuracy!.Value - 0.5) < 0.0001, $"{name}: accuracy {props.Accuracy}");
    Check(Math.Abs(props.Velocity!.Value - 2.7) < 0.0001, $"{name}: velocity {props.Velocity}");
    Check(props.Recoil == -6, $"{name}: recoil changed to {props.Recoil}");
    Check(props.Loudness == -20, $"{name}: loudness changed to {props.Loudness}");
}

Check(suppressorB.Properties!.DurabilityBurnModificator == 1.6, "excluded suppressor was modified");
Check(categoryNode.Properties!.DurabilityBurnModificator == 1.6, "category node was modified");
Check(unrelatedMuzzle.Properties!.DurabilityBurnModificator == 1.6, "unrelated muzzle was modified");

var set = new StatAdjustment { Mode = AdjustmentMode.Set, Value = 4 };
var unchanged = new StatAdjustment { Mode = AdjustmentMode.Unchanged, Value = 99 };
var scale = new StatAdjustment { Mode = AdjustmentMode.ScalePenaltyAboveNeutral, Value = 0.5 };
Check(set.Apply(2) == 4, "Set adjustment failed");
Check(unchanged.Apply(2) == 2, "Unchanged adjustment failed");
Check(scale.Apply(0.8, 1) == 0.8, "penalty scale changed a beneficial value below neutral");

var legacyConfig = JsonSerializer.Deserialize<Config>("""{"Accuracy":6,"Velocity":4}""");
Check(legacyConfig?.Accuracy.Mode == AdjustmentMode.Set && legacyConfig.Accuracy.Value == 6,
    "legacy numeric Accuracy setting did not migrate to Set");
Check(legacyConfig?.Velocity.Mode == AdjustmentMode.Set && legacyConfig.Velocity.Value == 4,
    "legacy numeric Velocity setting did not migrate to Set");

if (failures.Count > 0)
{
    Console.Error.WriteLine($"FAIL ({failures.Count}):");
    failures.ForEach(failure => Console.Error.WriteLine("  - " + failure));
    return 1;
}

Console.WriteLine($"OK: {patchedIds.Count} suppressors patched; additive, proportional, descendant and ID rules passed.");
return 0;
