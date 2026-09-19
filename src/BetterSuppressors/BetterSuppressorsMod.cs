using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace BetterSuppressors;

// Run after custom item loaders so suppressors contributed by other mods are present.
[Injectable(TypePriority = OnLoadOrder.PostLoad + 1000)]
public class BetterSuppressorsMod(
    TemplateTable templateTable,
    ISptLogger<BetterSuppressorsMod> logger) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var config = Config.Load(out var source);
        var patchedIds = SuppressorPatch.Apply(templateTable.Items.Values, config);

        logger.Info($"[BetterSuppressors] patched {patchedIds.Count} suppressors from {source}");
        logger.Debug($"[BetterSuppressors] patched item IDs: {string.Join(", ", patchedIds)}");

        if (patchedIds.Count == 0)
        {
            logger.Warning("[BetterSuppressors] patched nothing -- the item database was empty at this point.");
        }

        return Task.CompletedTask;
    }
}
