# Increase Tumble Damage

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for **R.E.P.O.** that scales tumble launch damage based on your upgrade level and provides configurable character stat overrides.

## Features

### 🎯 Tumble Launch Damage Scaling

**Deal more damage to enemies** as you upgrade your Tumble Launch skill. The damage multiplier scales with your upgrade level:

| Level | Multiplier (default 1.1×) | Base 15 damage → |
|-------|--------------------------|------------------|
| 1     | 1.1×                     | 16               |
| 3     | 3.3×                     | 49               |
| 5     | 5.5×                     | 82               |
| 10    | 11×                      | 165              |

> Only tumble launch damage is affected. Guns, items, and other damage sources are **not** scaled.

### 🛡️ Self-Damage Reduction

**Take less self-damage** from tumble impacts as you level up. Reduction scales linearly up to a configurable maximum:

- Default: up to **100% reduction** at **10 upgrades**
- At 5 upgrades → 50% reduction
- Fully configurable via config

### 📊 Character Stat Overrides

Override any of the 12 character stats via config. Default is `0` for all stats (no change — game's natural values preserved):

Health, Sprint Speed, Map Player Count, Energy, Extra Jump, Grab Range, Grab Strength, Tumble Launch, Crouch Rest, Tumble Wings, Tumble Climb, Death Head Battery

## Configuration

All settings are in `BepInEx/config/headclef.IncreaseTumbleDamage.cfg`.

### [Damage On Enemy]

| Key | Default | Description |
|-----|---------|-------------|
| Enable | `true` | Toggle enemy damage scaling on/off |
| Base Damage | `0` | Override tumble hit damage (0 = game default) |
| Multiplier Per Level | `1.1` | Multiplier per upgrade level. Formula: `level × value` |
| Max Multiplier | `0` | Cap the multiplier (0 = no cap) |

### [Damage On Player]

| Key | Default | Description |
|-----|---------|-------------|
| Enable | `true` | Toggle self-damage reduction on/off |
| Upgrades Needed For Max Reduction | `10` | Upgrades to reach full reduction |
| Max Damage Reduction | `1.0` | Maximum reduction ratio (1.0 = 100%) |

### [Character Stats]

| Key | Default | Description |
|-----|---------|-------------|
| Health | `0` | Health upgrades override |
| Sprint Speed | `0` | Sprint Speed upgrades override |
| Map Player Count | `0` | Map Player Count upgrades override |
| Energy | `0` | Energy (Stamina) upgrades override |
| Extra Jump | `0` | Extra Jump upgrades override |
| Grab Range | `0` | Grab Range upgrades override |
| Grab Strength | `0` | Grab Strength upgrades override |
| Tumble Launch | `0` | Tumble Launch upgrades override |
| Crouch Rest | `0` | Crouch Rest upgrades override |
| Tumble Wings | `0` | Tumble Wings upgrades override |
| Tumble Climb | `0` | Tumble Climb upgrades override |
| Death Head Battery | `0` | Death Head Battery upgrades override |

> All stat overrides default to `0`, meaning **no change**. Set a value to override the game's natural upgrade level.

## Multiplayer Behavior

- **Enemy damage scaling** runs on the **host**. If the host has the mod, all players' tumble damage is scaled. Clients without the mod still benefit.
- **Self-damage reduction** runs **per-client**. Only the player with the mod installed gets reduced self-damage.
- **Stat overrides** are applied on the **host**. The host's config determines stat values for the session.

> **Recommendation:** For the best experience, the **host** should have the mod installed.

## Requirements

- [BepInEx 5.x](https://github.com/BepInEx/BepInEx) installed for R.E.P.O.

## Installation

1. Download the latest release.
2. Place `Increase Tumble Damage.dll` into your `BepInEx/plugins` folder.
3. Launch R.E.P.O. — a config file is generated on first run.
4. Edit `BepInEx/config/headclef.IncreaseTumbleDamage.cfg` or from In-Game Mod Config Menu to customize.

> **Note:** If updating from a previous version, delete the old config file to get new default values.

## Development

### Project Structure
```
├── Increase Tumble Damage.cs          # Plugin entry point & config
├── Patches/
│   ├── StatOverridePatch.cs           # Harmony postfix — stat overrides
│   └── TumbleLaunchDamagePatch.cs     # Harmony prefixes — damage scaling
└── README.md
```

### Building
```bash
dotnet build
```

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
