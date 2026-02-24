# Increase Tumble Damage

Make your tumble launch hits **actually scale with your upgrades!**

In vanilla R.E.P.O., upgrading Tumble Launch only makes you fly further — the damage stays the same no matter how many upgrades you buy. This mod changes that by making your tumble damage grow stronger with each Tumble Launch upgrade you purchase.

---

## What This Mod Does

### 💥 Stronger Tumble Hits

Every Tumble Launch upgrade you buy makes your tumble hits deal more damage to enemies. By default, each upgrade level multiplies your damage by 1.1×:

- **Level 1** → 1.1× damage
- **Level 5** → 5.5× damage
- **Level 10** → 11× damage

So if a normal tumble hit does 15 damage at level 10 it would do **165 damage** instead!

### 🛡️ Less Self-Damage

Tumbling at high speed hurts you too, right? Not anymore. As you upgrade Tumble Launch, the self-damage you take from impacts is reduced. By default, at 10 upgrades you take **zero self-damage** from tumble impacts.

### 📊 Stat Overrides (Optional)

Want to start with extra health, sprint speed or any other stat? This mod lets you override all 12 character stats through the config file. All overrides are disabled by default (set to 0).

---

## Configuration

All settings can be changed in the config file at:
`BepInEx/config/headclef.IncreaseTumbleDamage.cfg`

Or through the **In-Game Mod Config Menu** if you have one installed.

### Enemy Damage Settings

- **Enable** — Turn enemy damage scaling on or off (default: on)
- **Multiplier Per Level** — How much each upgrade multiplies your damage (default: 1.1)
- **Max Multiplier** — Set a cap on the multiplier, or leave at 0 for unlimited scaling (default: 0)

### Self-Damage Reduction Settings

- **Enable** — Turn self-damage reduction on or off (default: on)
- **Upgrades Needed For Max Reduction** — How many upgrades you need for full protection (default: 10)
- **Max Damage Reduction** — Maximum reduction percentage. 1.0 = 100%, 0.75 = 75% (default: 1.0)

### Stat Override Settings

Override any of these stats by setting a value other than 0:
Health, Sprint Speed, Map Player Count, Energy, Extra Jump, Grab Range, Grab Strength, Tumble Launch, Crouch Rest, Tumble Wings, Tumble Climb, Death Head Battery

---

## Multiplayer

- **Host has the mod installed** → Everyone in the lobby benefits from the damage scaling. Other players don't need the mod.
- **Self-damage reduction** works individually — only players who have the mod installed get reduced self-damage.
- For the best experience, we recommend the **host** installs the mod.

---

## Installation

1. Make sure [BepInEx 5.x](https://github.com/BepInEx/BepInEx) is installed for R.E.P.O.
2. Download the mod and place `Increase Tumble Damage.dll` into your `BepInEx/plugins` folder.
3. Launch the game — the config file will be created automatically.
4. Adjust settings in the config file or in-game config menu.

> **Updating from a previous version?** Delete the old config file at `BepInEx/config/headclef.IncreaseTumbleDamage.cfg` to get the latest default values.

---

## Compatibility

- Works with other mods (tested alongside health bars, health regen, shop mods, etc.)
- Only affects tumble launch damage — does **not** change gun damage, item damage, or any other damage source
- Safe to use in multiplayer — non-modded players won't experience issues
