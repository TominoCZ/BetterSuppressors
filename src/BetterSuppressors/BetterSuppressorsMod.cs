using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace BetterSuppressors;

// Runs ahead of the handbook, trader, preset and flea callbacks (500000-900000) so
// nothing derives from the vanilla numbers. The database is loaded by the startup
// hosted service before any IOnLoad runs, so being this early is safe.
[Injectable(TypePriority = 1000)]
public class BetterSuppressorsMod(
    TemplateTable templateTable,
    ISptLogger<BetterSuppressorsMod> logger) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var config = Config.Load(out var source);
        var patched = SuppressorPatch.Apply(templateTable.Items.Values, config);

        logger.Info($"[BetterSuppressors] patched {patched} suppressors from {source} " +
                    $"(erg {config.Ergonomics}, burn {config.DurabilityBurn}, heat {config.HeatFactor}, " +
                    $"cool {config.CoolFactor}, accuracy {config.Accuracy}, velocity {config.Velocity})");

        if (patched == 0)
        {
            logger.Warning("[BetterSuppressors] patched nothing -- the item database was empty at this point.");
        }

        return Task.CompletedTask;
    }
}
