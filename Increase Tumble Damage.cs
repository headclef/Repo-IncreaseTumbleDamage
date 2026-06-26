using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Increase_Tumble_Damage;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("headclef.CharacterStats", BepInDependency.DependencyFlags.HardDependency)]
public class Increase_Tumble_Damage : BaseUnityPlugin
{
    private const string PluginGuid = "headclef.IncreaseTumbleDamage";
    private const string PluginName = "Increase Tumble Damage";
    private const string PluginVersion = "1.2.1";

    internal static Increase_Tumble_Damage Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    // ── Enemy Damage Scaling Config ──
    public static ConfigEntry<bool> EnableDamageOnEnemy = null!;
    public static ConfigEntry<float> MultiplierPerLevel = null!;
    public static ConfigEntry<float> MaxMultiplier = null!;

    // ── Self-Damage Reduction Config ──
    public static ConfigEntry<bool> EnableDamageOnPlayer = null!;
    public static ConfigEntry<int> UpgradesNeededForMaxReduction = null!;
    public static ConfigEntry<float> MaxDamageReduction = null!;

    private void Awake()
    {
        Instance = this;

        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        BindConfiguration();
        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    private void BindConfiguration()
    {
        // Enemy damage scaling
        const string enemySection = "Damage On Enemy";
        EnableDamageOnEnemy = Config.Bind(enemySection, "Enable", true,
            "Enable scaling of damage dealt to enemies when tumble-hitting them.");
        MultiplierPerLevel = Config.Bind(enemySection, "Multiplier Per Level", 1.1f,
            "Damage multiplier per Tumble Launch upgrade level. Final multiplier = level × this value. E.g. 1.1 means level 1 = 1.1×, level 10 = 11×.");
        MaxMultiplier = Config.Bind(enemySection, "Max Multiplier", 0f,
            "Maximum damage multiplier cap. 0 = no cap (unlimited scaling).");

        // Self-damage reduction
        const string playerSection = "Damage On Player";
        EnableDamageOnPlayer = Config.Bind(playerSection, "Enable", true,
            "Enable reduction of self-damage from tumble impacts based on upgrade level.");
        UpgradesNeededForMaxReduction = Config.Bind(playerSection, "Upgrades Needed For Max Reduction", 10,
            "Number of Tumble Launch upgrades required to reach maximum self-damage reduction.");
        MaxDamageReduction = Config.Bind(playerSection, "Max Damage Reduction", 1.0f,
            "Maximum self-damage reduction ratio from tumble impacts. E.g. 0.75 = 75% reduction (100 dmg -> 25).");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }
}