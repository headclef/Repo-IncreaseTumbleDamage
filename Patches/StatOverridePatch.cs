using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace Increase_Tumble_Damage.Patches;

[HarmonyPatch]
internal static class StatOverridePatch
{
    private static bool _hasApplied;

    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.Start))]
    [HarmonyPostfix]
    private static void GameDirector_Start_Postfix()
    {
        Object.FindObjectOfType<MonoBehaviour>().StartCoroutine(ApplyStatOverrides());
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.ResetProgress))]
    [HarmonyPostfix]
    private static void RunManager_ResetProgress_Postfix()
    {
        _hasApplied = false;
    }

    private static IEnumerator ApplyStatOverrides()
    {
        // Wait until the level has finished generating.
        while (!SemiFunc.LevelGenDone())
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (_hasApplied || !SemiFunc.RunIsLevel() || !SemiFunc.IsMasterClientOrSingleplayer())
            yield break;

        foreach (var player in SemiFunc.PlayerGetAll())
        {
            var steamId = SemiFunc.PlayerGetSteamID(player);
            var upgrades = StatsManager.instance.FetchPlayerUpgrades(steamId);

            ApplyIfNonZero(Increase_Tumble_Damage.HealthBonus, "Health", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerHealth(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.SpeedBonus, "Speed", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerSprintSpeed(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.MapCountBonus, "Map Player Count", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradeMapPlayerCount(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.EnergyBonus, "Stamina", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerEnergy(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.ExtraJumpBonus, "Extra Jump", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerExtraJump(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.GrabRangeBonus, "Range", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerGrabRange(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.GrabStrengthBonus, "Strength", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerGrabStrength(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.TumbleLaunchBonus, "Launch", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerTumbleLaunch(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.CrouchRestBonus, "Crouch Rest", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerCrouchRest(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.TumbleWingsBonus, "Tumble Wings", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerTumbleWings(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.TumbleClimbBonus, "Tumble Climb", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradePlayerTumbleClimb(id, delta));

            ApplyIfNonZero(Increase_Tumble_Damage.DeathHeadBonus, "Death Head", steamId, upgrades,
                (id, delta) => PunManager.instance.UpgradeDeathHeadBattery(id, delta));
        }

        _hasApplied = true;
        Increase_Tumble_Damage.Logger.LogInfo("Stat overrides applied.");
    }

    /// <summary>
    /// Applies an upgrade only when the configured value is non-zero.
    /// Calculates the delta between desired and current level, then calls the upgrade method.
    /// </summary>
    private static void ApplyIfNonZero(
        BepInEx.Configuration.ConfigEntry<int> config,
        string upgradeKey,
        string steamId,
        System.Collections.Generic.Dictionary<string, int> upgrades,
        System.Action<string, int> upgradeMethod)
    {
        if (config.Value == 0)
            return;

        int current = upgrades.ContainsKey(upgradeKey) ? upgrades[upgradeKey] : 0;
        int delta = config.Value - current;

        if (delta != 0)
        {
            upgradeMethod(steamId, delta);
            Increase_Tumble_Damage.Logger.LogDebug($"Applied {upgradeKey}: {current} -> {config.Value} (delta: {delta})");
        }
    }
}
