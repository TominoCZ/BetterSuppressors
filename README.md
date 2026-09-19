<img src="BetterSuppressors.png" width="180" align="right" alt="BetterSuppressors">

# BetterSuppressors

A configurable SPT suppressor rebalance that also covers suppressors added by other mods.

The default preset keeps each suppressor's authored ergonomics, heat, cooling, recoil and
loudness. It halves only the extra durability-burn penalty, then adds a modest 1.5 accuracy
and 2 velocity points. Individual suppressors remain different instead of being flattened
to one set of statistics.

## What it changes

For example, a suppressor with `1.60` durability burn becomes `1.30`: its additional
60% wear is reduced to 30%, not removed. A suppressor with −1 accuracy and +0.7 velocity
becomes +0.5 and +2.7 respectively.

The patch runs after custom item registration. It covers direct children of SPT's suppressor
category, items below mod-added suppressor subcategories, and optional explicit item IDs.
Specific IDs can also be excluded. Recoil and loudness are left alone by default.

## Install

Requires SPT `4.1.5`.

Extract the release zip into your SPT folder. That puts `BetterSuppressors.dll` and
`config.json` here:

```
<SPT>/SPT_Runtime/user/mods/BetterSuppressors/
```

Restart the server. It reports what it did on startup:

```
[BetterSuppressors] patched 91 suppressors from config.json: 5a..., 68c..., ...
```

This is a server mod — it does not go in `BepInEx/plugins`.

## Configuring

Edit `config.json` and restart the server. No rebuild needed.

Each statistic accepts one of four modes:

- `Unchanged` keeps the authored value;
- `Set` replaces it;
- `Add` adds the configured value;
- `ScalePenaltyAboveNeutral` scales the distance from a neutral value. Durability burn,
  heat and cooling use `1` as neutral; the other statistics use `0`.

The shipped configuration changes only durability burn, accuracy and velocity:

```json
{
  "Ergonomics": { "Mode": "Unchanged", "Value": 0 },
  "DurabilityBurn": { "Mode": "ScalePenaltyAboveNeutral", "Value": 0.5 },
  "HeatFactor": { "Mode": "Unchanged", "Value": 0 },
  "CoolFactor": { "Mode": "Unchanged", "Value": 0 },
  "Accuracy": { "Mode": "Add", "Value": 1.5 },
  "Velocity": { "Mode": "Add", "Value": 2.0 },
  "IncludeDescendants": true,
  "AdditionalItemIds": [],
  "ExcludedItemIds": []
}
```

Use `AdditionalItemIds` for an integrally suppressed or unusually categorized mod item that
does not inherit from the suppressor category. Use `ExcludedItemIds` to leave a particular
item untouched.

## Building

Needs the .NET 10 SDK and the path to your SPT server folder.

`package.sh` picks the path up from `.env`:

```bash
cp .env.example .env    # then edit SPT_RUNTIME
./package.sh            # builds the release zip
```

Everything else needs it as a real environment variable, since MSBuild can't read `.env`:

```bash
export SPT_RUNTIME=/path/to/SPT/SPT_Runtime

dotnet build                                            # compile
dotnet build -p:SptDeploy=true                          # install into your server
dotnet run --project tests/BetterSuppressors.SelfCheck  # run the checks
```

## Licence

[CC BY-NC-SA 4.0](LICENSE), matching SPT and Fika.
