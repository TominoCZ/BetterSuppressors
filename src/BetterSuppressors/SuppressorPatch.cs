using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace BetterSuppressors;

public static class SuppressorPatch
{
    public static readonly MongoId SilencerParent = new("550aa4cd4bdc2dd8348b456c");

    public static int Apply(IEnumerable<TemplateItem> items, Config config)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(config);

        var patched = 0;

        foreach (var item in items)
        {
            // "Node" entries are category placeholders, not real items.
            if (item.Parent != SilencerParent || item.Type != "Item")
            {
                continue;
            }

            var props = item.Properties;
            if (props is null)
            {
                continue;
            }

            props.Ergonomics = config.Ergonomics;
            props.DurabilityBurnModificator = config.DurabilityBurn;
            props.HeatFactor = config.HeatFactor;
            props.CoolFactor = config.CoolFactor;
            props.Accuracy = config.Accuracy;
            props.Velocity = config.Velocity;

            patched++;
        }

        return patched;
    }
}
