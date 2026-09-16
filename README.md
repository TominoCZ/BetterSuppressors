# BetterSuppressors

A mod for SPT that makes suppressors actually useful.

In vanilla Tarkov a suppressor is a trade: you buy quiet with ergonomics, durability
burn and heat. This mod removes that trade and turns the can into what a big lump of
steel on the muzzle ought to be — neutral on handling, easier on the barrel, and a
genuine help at range.

Recoil and loudness are left alone; those were already in your favour.

## What it changes

Every suppressor in the database (the 76 items under the `Silencer` node) gets:

| Property | Vanilla range | Default here | Effect |
|---|---|---|---|
| `Ergonomics` | −30 … −2 | `0` | handling penalty gone |
| `DurabilityBurnModificator` | 1.2 … 2.15 | `1.0` | barrel wears as if unsuppressed |
| `HeatFactor` | 1.05 … 1.34 | `0.90` | generates less heat |
| `CoolFactor` | 1.0 … 1.2 | `0.92` | sheds heat faster |
| `Accuracy` | −5 … +1 | `+6` | tighter groups |
| `Velocity` | +0.2 … +1.2 | `+6` | flatter trajectory |

### Two things worth knowing

**`CoolFactor` runs backwards from how it reads.** A value above 1 is a *penalty*, not
a bonus. The proof is in the vanilla data: the parts with `CoolFactor` below 1 are the
long heavy barrels — Mosin 730mm, PKP, M700 heavy, AX .308 — all sitting near
`0.82` heat / `0.86` cool. Those are the game's own "lots of steel, handles heat well"
components. Lower is better for both fields, and vanilla suppressors are penalised on
both. If you tune these, tune them downward.

**There is no range-gated stat.** `EffectiveDistance` exists on items but is `0` on all
4673 of them — dead, wired to nothing. `SightingRange` is just a sight's zeroing value.
So "better at medium range" is not something the database can be told directly. It
falls out of `Velocity`: a flatter arc is invisible at 50 m and worth real drop and
time-of-flight compensation by 350 m. `Accuracy` behaves the same way, since a fixed
MOA change is 3.5 cm at 100 m and 12 cm at 350 m.

## Install

Extract the release zip into your SPT folder, or drop `BetterSuppressors.dll` and
`config.json` into:

```
<SPT>/SPT_Runtime/user/mods/BetterSuppressors/
```

This is a server mod. It does not go in `BepInEx/plugins` -- there is no client-side
component, and the server mod loader does not look there.

Restart the server. It logs what it did:

```
[BetterSuppressors] patched 76 suppressors from config.json (erg 0, burn 1, heat 0.9, ...)
```

That count is whatever your database actually holds — 76 on stock SPT 4.1.5, more if
another mod adds suppressors of its own, which this mod will happily patch too.

Requires SPT `~4.1.5`.

## Configuring

Edit `config.json` next to the DLL and restart the server. No rebuild needed.

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

For reference when tuning: `+6` velocity takes 5.56 from roughly 880 to 933 m/s. Real
suppressors are worth about +1–3%, which is close to vanilla; +6 is a deliberate
gameplay buff. On heat, `0.82`/`0.86` would make a suppressor as good a heat sink as
the heaviest barrels in the game.

A missing or malformed `config.json` falls back to the built-in defaults rather than
taking the server down during load.

## Building

Needs the .NET 10 SDK. Point the build at your SPT server folder — the one holding
`SPTarkov.Server.Core.dll`:

```bash
export SPT_RUNTIME=/path/to/SPT/SPT_Runtime
dotnet build
```

Build straight into the server:

```bash
dotnet build src/BetterSuppressors -p:SptDeploy=true
```

That copies the DLL every time but will not overwrite a `config.json` you have already
tuned.

## Packaging a release

```bash
export SPT_RUNTIME=/path/to/SPT/SPT_Runtime
./package.sh
```

Builds Release and writes `BetterSuppressors_<version>.zip` next to the script, laid out
so it extracts straight into the SPT folder. The version comes from `<Version>` in the
csproj, which also stamps the assembly, so the filename cannot drift from what the server
reports on load.

## Tests

```bash
dotnet run --project tests/BetterSuppressors.SelfCheck
```

Asserts that the patch hits only real suppressors — skipping the category `Node`, the
neighbouring flash hiders, and entries with no properties — and that it leaves recoil
and loudness alone. Exits non-zero on failure.

## Licence

[CC BY-NC-SA 4.0](LICENSE), matching SPT and Fika.
