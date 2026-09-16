using SPTarkov.Server.Core.Models.Spt.Mod;
using SemRange = SemanticVersioning.Range;
using SemVersion = SemanticVersioning.Version;

namespace BetterSuppressors;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.tominocz.bettersuppressors";
    public string Name { get; init; } = "BetterSuppressors";
    public string Author { get; init; } = "TominoCZ";
    public List<string>? Contributors { get; init; } = [];
    public SemVersion Version { get; init; } = AssemblyVersion();
    public SemRange SptVersion { get; init; } = new("~4.1.5");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; } = [];
    public Dictionary<string, SemRange>? ModDependencies { get; init; } = [];
    public string? Url { get; init; } = "https://github.com/TominoCZ/BetterSuppressors";
    public string License { get; init; } = "CC BY-NC-SA 4.0";

    // Version lives in the csproj so the assembly, the server listing and the release
    // zip can't disagree.
    private static SemVersion AssemblyVersion()
    {
        var v = typeof(ModMetadata).Assembly.GetName().Version ?? new Version(0, 0, 0);
        return new SemVersion(v.Major, Math.Max(v.Minor, 0), Math.Max(v.Build, 0));
    }
}
