using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace BetterSuppressors;

public static class SuppressorPatch
{
    public static readonly MongoId SilencerParent = new("550aa4cd4bdc2dd8348b456c");

    public static IReadOnlyList<MongoId> Apply(IEnumerable<TemplateItem> items, Config config)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(config);

        var itemList = items.ToList();
        var byId = itemList.ToDictionary(item => item.Id, item => item);
        var additionalIds = config.AdditionalItemIds.Select(id => new MongoId(id)).ToHashSet();
        var excludedIds = config.ExcludedItemIds.Select(id => new MongoId(id)).ToHashSet();
        var patched = new List<MongoId>();

        foreach (var item in itemList)
        {
            if (item.Type != "Item" || item.Properties is null || excludedIds.Contains(item.Id))
            {
                continue;
            }

            var isSuppressor = item.Parent == SilencerParent
                || additionalIds.Contains(item.Id)
                || (config.IncludeDescendants && IsDescendantOf(item, SilencerParent, byId));

            if (!isSuppressor)
            {
                continue;
            }

            var props = item.Properties;
            props.Ergonomics = config.Ergonomics.Apply(props.Ergonomics);
            props.DurabilityBurnModificator = config.DurabilityBurn.Apply(props.DurabilityBurnModificator, 1);
            props.HeatFactor = config.HeatFactor.Apply(props.HeatFactor, 1);
            props.CoolFactor = config.CoolFactor.Apply(props.CoolFactor, 1);
            props.Accuracy = config.Accuracy.Apply(props.Accuracy);
            props.Velocity = config.Velocity.Apply(props.Velocity);
            patched.Add(item.Id);
        }

        return patched;
    }

    private static bool IsDescendantOf(
        TemplateItem item,
        MongoId ancestorId,
        IReadOnlyDictionary<MongoId, TemplateItem> byId)
    {
        var visited = new HashSet<MongoId>();
        var parentId = item.Parent;

        while (visited.Add(parentId) && byId.TryGetValue(parentId, out var parent))
        {
            if (parent.Parent == ancestorId)
            {
                return true;
            }

            parentId = parent.Parent;
        }

        return false;
    }
}
