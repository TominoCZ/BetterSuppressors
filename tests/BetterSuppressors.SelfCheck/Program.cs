using BetterSuppressors;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

// dotnet run --project tests/BetterSuppressors.SelfCheck

var flashHider = new MongoId("550aa4bf4bdc2dd6348b456b");

static TemplateItem Item(MongoId parent, string type, TemplateItemProperties? props) => new()
{
    Id = new MongoId(),
    Name = "test",
    Parent = parent,
    Type = type,
    Properties = props,
};

static TemplateItemProperties Stock() => new()
{
    Ergonomics = -16,
    DurabilityBurnModificator = 1.63,
    HeatFactor = 1.14,
    CoolFactor = 1.03,
    Accuracy = -1,
    Velocity = 0.7,
    Recoil = -6,
    Loudness = -20,
};

var suppressorA = Item(SuppressorPatch.SilencerParent, "Item", Stock());
var suppressorB = Item(SuppressorPatch.SilencerParent, "Item", Stock());
var categoryNode = Item(SuppressorPatch.SilencerParent, "Node", Stock());
var unrelatedMuzzle = Item(flashHider, "Item", Stock());
var brokenEntry = Item(SuppressorPatch.SilencerParent, "Item", null);

var db = new[] { suppressorA, suppressorB, categoryNode, unrelatedMuzzle, brokenEntry };
var config = new Config();

var patched = SuppressorPatch.Apply(db, config);

var failures = new List<string>();
void Check(bool ok, string what)
{
    if (!ok) failures.Add(what);
}

// The Node, the flash hider and the propertyless entry are all skipped.
Check(patched == 2, $"expected 2 patched, got {patched}");

foreach (var (name, s) in new[] { ("A", suppressorA), ("B", suppressorB) })
{
    var p = s.Properties!;
    Check(p.Ergonomics == config.Ergonomics, $"{name}: ergonomics {p.Ergonomics}");
    Check(p.DurabilityBurnModificator == config.DurabilityBurn, $"{name}: burn {p.DurabilityBurnModificator}");
    Check(p.HeatFactor == config.HeatFactor, $"{name}: heat {p.HeatFactor}");
    Check(p.CoolFactor == config.CoolFactor, $"{name}: cool {p.CoolFactor}");
    Check(p.Accuracy == config.Accuracy, $"{name}: accuracy {p.Accuracy}");
    Check(p.Velocity == config.Velocity, $"{name}: velocity {p.Velocity}");
    Check(p.Recoil == -6, $"{name}: recoil was modified ({p.Recoil})");
    Check(p.Loudness == -20, $"{name}: loudness was modified ({p.Loudness})");
}

Check(categoryNode.Properties!.Ergonomics == -16, "category Node was modified");
Check(unrelatedMuzzle.Properties!.Ergonomics == -16, "flash hider was modified");

// Heat fields are penalties in vanilla, so the defaults have to come in under 1.
Check(config.HeatFactor < 1.0, $"HeatFactor default {config.HeatFactor} is not a help");
Check(config.CoolFactor < 1.0, $"CoolFactor default {config.CoolFactor} is not a help");

if (failures.Count > 0)
{
    Console.Error.WriteLine($"FAIL ({failures.Count}):");
    failures.ForEach(f => Console.Error.WriteLine("  - " + f));
    return 1;
}

Console.WriteLine($"OK: {patched} suppressors patched, siblings untouched, null properties survived.");
return 0;
