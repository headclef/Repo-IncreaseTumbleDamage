using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Increase_Tumble_Damage;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Increase_Tumble_Damage : BaseUnityPlugin
{
    private const string PluginGuid = "headclef.IncreaseTumbleDamage";
    private const string PluginName = "Increase Tumble Damage";
    private const string PluginVersion = "1.0.5";

    internal static Increase_Tumble_Damage Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    internal static bool PatchFailed { get; set; }

    // ── Stat Config Entries (default 0 = no change) ──
    internal static ConfigEntry<int> HealthBonus = null!;
    internal static ConfigEntry<int> SpeedBonus = null!;
    internal static ConfigEntry<int> MapCountBonus = null!;
    internal static ConfigEntry<int> EnergyBonus = null!;
    internal static ConfigEntry<int> ExtraJumpBonus = null!;
    internal static ConfigEntry<int> GrabRangeBonus = null!;
    internal static ConfigEntry<int> GrabStrengthBonus = null!;
    internal static ConfigEntry<int> TumbleLaunchBonus = null!;
    internal static ConfigEntry<int> CrouchRestBonus = null!;
    internal static ConfigEntry<int> TumbleWingsBonus = null!;
    internal static ConfigEntry<int> TumbleClimbBonus = null!;
    internal static ConfigEntry<int> DeathHeadBonus = null!;

    // ── Damage Scaling Config Entries ──
    internal static ConfigEntry<bool> EnableDamageOnEnemy = null!;
    internal static ConfigEntry<int> TumbleDamageOnHitEnemy = null!;
    internal static ConfigEntry<float> MultiplierPerLevel = null!;
    internal static ConfigEntry<float> MaxMultiplier = null!;

    internal static ConfigEntry<bool> EnableDamageOnPlayer = null!;
    internal static ConfigEntry<int> UpgradesNeededForMaxReduction = null!;
    internal static ConfigEntry<float> MaxDamageReduction = null!;

    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        BindConfiguration();
        Patch();

        if (PatchFailed)
        {
            Logger.LogError("One or more patches failed to apply. Check logs above for details.");
        }
        else
        {
            Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        }
    }

    private void BindConfiguration()
    {
        const string statsSection = "Stats";

        // Stats — 0 means "don't change this stat, use the game's value"
        HealthBonus = Config.Bind(statsSection, "Health", 0,
            "Override for Health upgrades. 0 = no change (use game default).");
        SpeedBonus = Config.Bind(statsSection, "Sprint Speed", 0,
            "Override for Sprint Speed upgrades. 0 = no change.");
        MapCountBonus = Config.Bind(statsSection, "Player Map Count", 0,
            "Override for Player Map Count upgrades. 0 = no change.");
        EnergyBonus = Config.Bind(statsSection, "Energy", 0,
            "Override for Energy upgrades. 0 = no change.");
        ExtraJumpBonus = Config.Bind(statsSection, "Extra Jump", 0,
            "Override for Extra Jump upgrades. 0 = no change.");
        GrabRangeBonus = Config.Bind(statsSection, "Grab Range", 0,
            "Override for Grab Range upgrades. 0 = no change.");
        GrabStrengthBonus = Config.Bind(statsSection, "Grab Strength", 0,
            "Override for Grab Strength upgrades. 0 = no change.");
        TumbleLaunchBonus = Config.Bind(statsSection, "Tumble Launch", 0,
            "Override for Tumble Launch upgrades. 0 = no change.");
        CrouchRestBonus = Config.Bind(statsSection, "Crouch Rest", 0,
            "Override for Crouch Rest upgrades. 0 = no change.");
        TumbleWingsBonus = Config.Bind(statsSection, "Tumble Wings", 0,
            "Override for Tumble Wings upgrades. 0 = no change.");
        TumbleClimbBonus = Config.Bind(statsSection, "Tumble Climb", 0,
            "Override for Tumble Climb upgrades. 0 = no change.");
        DeathHeadBonus = Config.Bind(statsSection, "Death Head Battery", 0,
            "Override for Death Head Battery upgrades. 0 = no change.");

        // Damage to enemy scaling
        const string enemySection = "Damage On Enemy";
        EnableDamageOnEnemy = Config.Bind(enemySection, "Enable", true,
            "Enable scaling of damage dealt to enemies when tumble-hitting them.");
        TumbleDamageOnHitEnemy = Config.Bind(enemySection, "Base Damage", 0,
            "Base damage when hitting an enemy while tumbling. 0 = use game's default value (5).");
        MultiplierPerLevel = Config.Bind(enemySection, "Multiplier Per Level", 1.1f,
            "Damage multiplier per Tumble Launch upgrade level. Final multiplier = level × this value. E.g. 1.1 means level 1 = 1.1×, level 10 = 11×.");
        MaxMultiplier = Config.Bind(enemySection, "Max Multiplier", 0f,
            "Maximum damage multiplier cap. 0 = no cap (unlimited scaling).");

        // Self-damage reduction scaling
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