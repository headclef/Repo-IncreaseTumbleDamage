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
    private const string PluginVersion = "1.0.0";

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
    internal static ConfigEntry<int> TumbleDamageOnHitEnemy = null!;
    internal static ConfigEntry<int> DamagePerUpgradeLevel = null!;
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
        const string damageSection = "Damage Scaling";

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

        // Damage scaling
        TumbleDamageOnHitEnemy = Config.Bind(damageSection, "Tumble Damage On Hit Enemy", 0,
            "Base damage when hitting an enemy while tumbling. 0 = use game's default value (5).");
        DamagePerUpgradeLevel = Config.Bind(damageSection, "Damage Per Upgrade Level", 2,
            "Additional damage dealt per Tumble Launch upgrade level. Scales the damage in HitEnemy.");
        UpgradesNeededForMaxReduction = Config.Bind(damageSection, "Upgrades Needed For Max Reduction", 8,
            "Number of Tumble Launch upgrades required to reach maximum self-damage reduction.");
        MaxDamageReduction = Config.Bind(damageSection, "Max Damage Reduction", 1.0f,
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