<img src="BetterSuppressors.png" width="180" align="right" alt="BetterSuppressors">

# BetterSuppressors

A mod for SPT that makes suppressors actually useful.

In vanilla, a suppressor charges you ergonomics, durability burn and heat in exchange for
being quiet. This mod drops that tax and makes the can help instead.

## What it changes

Every suppressor in the game (76 of them):

| | Vanilla | Here |
|---|---|---|
| Ergonomics | −30 … −2 | **0** |
| Durability burn | +20% … +115% | **none** |
| Heat | +5% … +34% | **−10%** |
| Cooling | up to 20% worse | **8% better** |
| Accuracy | −5 … +1 | **+6** |
| Muzzle velocity | +0.2 … +1.2 | **+6** |

Recoil and loudness are left alone — those were already in your favour.

The velocity bump is what makes suppressors worth it at range: it barely shows at 50 m,
but flattens the arc noticeably by 350 m.

## Install

Requires SPT `4.1.5`.

Extract the release zip into your SPT folder. That puts `BetterSuppressors.dll` and
`config.json` here:

```
<SPT>/SPT_Runtime/user/mods/BetterSuppressors/
```

Restart the server. It reports what it did on startup:

```
[BetterSuppressors] patched 76 suppressors from config.json (erg 0, burn 1, heat 0.9, ...)
```

This is a server mod — it does not go in `BepInEx/plugins`.

## Configuring

Edit `config.json` and restart the server. No rebuild needed.

```json
{
  "Ergonomics": 0,
  "DurabilityBurn": 1.0,
  "HeatFactor": 0.90,
  "CoolFactor": 0.92,
  "Accuracy": 6,
  "Velocity": 6
}
```

For the two heat values, **lower is better**. Both are penalties above `1.0` in vanilla,
and the game's heavy barrels — its best heat sinks — sit around `0.82` and `0.86`.

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
