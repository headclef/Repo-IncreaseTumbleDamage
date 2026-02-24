# Increase Tumble Damage

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for **R.E.P.O.** that provides configurable character stat overrides and scales tumble launch damage by skill level.

## Features

### Configurable Character Stats
All 12 character stats can be overridden via BepInEx config. **Default value is `0` for every stat, meaning no change** — the game's natural values are preserved unless you explicitly set an override.

| Config Key | Description |
|---|---|
| Health | Health upgrades |
| Sprint Speed | Sprint Speed upgrades |
| Player Map Count | Map Player Count upgrades |
| Energy | Energy (Stamina) upgrades |
| Extra Jump | Extra Jump upgrades |
| Grab Range | Grab Range upgrades |
| Grab Strength | Grab Strength upgrades |
| Tumble Launch | Tumble Launch upgrades |
| Crouch Rest | Crouch Rest upgrades |
| Tumble Wings | Tumble Wings upgrades |
| Tumble Climb | Tumble Climb upgrades |
| Death Head Battery | Death Head Battery upgrades |

### Tumble Launch Damage Scaling
Scales tumble launch damage based on the player's upgrade level:

| Config Key | Default | Description |
|---|---|---|
| Tumble Damage On Hit Enemy | `0` (game default) | Damage received when hitting an enemy while tumbling |
| Upgrades Needed For Max Reduction | `8` | Tumble Launch upgrades needed to reach max damage reduction |
| Max Damage Reduction | `1.0` | Maximum damage reduction ratio (e.g. `0.75` = 75% reduction) |

## Requirements
- [BepInEx 5.x](https://github.com/BepInEx/BepInEx) installed for R.E.P.O.

## Installation
1. Build the project (or grab the latest release).
2. Place `Increase_Tumble_Damage.dll` into your `BepInEx/plugins` folder.
3. Launch R.E.P.O. — a config file will be generated on first run.
4. Edit the config at `BepInEx/config/headclef.IncreaseTumbleDamage.cfg` to customize stats and damage scaling.

## Development

### Project Structure
```
├── Increase Tumble Damage.cs        # Plugin entry point & config bindings
├── Patches/
│   ├── StatOverridePatch.cs         # Harmony postfix — stat overrides
│   └── TumbleLaunchDamagePatch.cs   # Harmony transpilers — damage scaling
├── Reference/                       # Reference code (not compiled)
└── README.md
```

### Building
```bash
dotnet build
```

## License
This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
