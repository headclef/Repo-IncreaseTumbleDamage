using System;
using HarmonyLib;
using UnityEngine;

namespace Increase_Tumble_Damage.Patches;

[HarmonyPatch]
internal static class TumbleLaunchDamagePatch
{
    // Flag to track when we're inside a HurtCollider.EnemyHurt call.
    private static bool _inEnemyHurt;
    private static float _tumbleMultiplier = 1f;

    // ── Enemy Damage Scaling ──

    [HarmonyPatch(typeof(HurtCollider), "EnemyHurt")]
    [HarmonyPrefix]
    private static void EnemyHurt_Prefix(HurtCollider __instance)
    {
        _inEnemyHurt = false;

        if (!Increase_Tumble_Damage.EnableDamageOnEnemy.Value)
            return;

        try
        {
            // Only scale the player's tumble "Hurt Collider", not enemy attack colliders
            // (e.g. "Attack Vacuum Hurt Collider", "Attack Impact Hurt Collider", "Hurt Collider First Hit")
            if (__instance.gameObject.name != "Hurt Collider")
                return;

            // Get local player upgrades to calculate multiplier.
            var players = SemiFunc.PlayerGetAll();
            if (players == null || players.Count == 0)
                return;

            var localPlayer = players[0];
            string steamId = SemiFunc.PlayerGetSteamID(localPlayer);
            int tumbleUpgrades = StatsManager.instance.playerUpgradeLaunch[steamId];

            float multiplierPerLevel = Increase_Tumble_Damage.MultiplierPerLevel.Value;
            float maxMultiplier = Increase_Tumble_Damage.MaxMultiplier.Value;

            if (multiplierPerLevel <= 0f || tumbleUpgrades <= 0)
                return;

            // Formula: multiplier = level × MultiplierPerLevel
            // E.g. level 1 × 1.1 = 1.1, level 10 × 1.1 = 11
            _tumbleMultiplier = multiplierPerLevel * tumbleUpgrades;
            if (maxMultiplier > 0f)
                _tumbleMultiplier = Math.Min(_tumbleMultiplier, maxMultiplier);
            _inEnemyHurt = true;

            Increase_Tumble_Damage.Logger.LogInfo(
                $"EnemyHurt: GO={__instance.gameObject.name}, multiplier={_tumbleMultiplier:F2}, upgrades={tumbleUpgrades}, enemyDamage={__instance.enemyDamage}");
        }
        catch (Exception ex)
        {
            Increase_Tumble_Damage.Logger.LogError($"EnemyHurt prefix exception: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(HurtCollider), "EnemyHurt")]
    [HarmonyPostfix]
    private static void EnemyHurt_Postfix()
    {
        _inEnemyHurt = false;
    }

    /// <summary>
    /// Intercepts EnemyHealth.Hurt to scale the damage when called from HurtCollider.EnemyHurt.
    /// </summary>
    [HarmonyPatch(typeof(EnemyHealth), "Hurt")]
    [HarmonyPrefix]
    private static void EnemyHealth_Hurt_Prefix(ref int _damage, Vector3 _hurtDirection)
    {
        if (!_inEnemyHurt || _tumbleMultiplier <= 1f)
            return;

        int original = _damage;
        _damage = Math.Max((int)(_damage * _tumbleMultiplier), 1);

        Increase_Tumble_Damage.Logger.LogInfo(
            $"EnemyHealth.Hurt SCALED: damage {original} -> {_damage} (multiplier: {_tumbleMultiplier:F2})");
    }

    // ── Self-Damage Reduction ──

    [HarmonyPatch(typeof(PlayerTumble), nameof(PlayerTumble.BreakImpact))]
    [HarmonyPrefix]
    private static void BreakImpact_Prefix(PlayerTumble __instance)
    {
        if (!Increase_Tumble_Damage.EnableDamageOnPlayer.Value)
            return;

        if (__instance.impactHurtDamage <= 0)
            return;

        try
        {
            int upgradesNeeded = Increase_Tumble_Damage.UpgradesNeededForMaxReduction.Value;
            float maxReduction = Increase_Tumble_Damage.MaxDamageReduction.Value;

            if (maxReduction <= 0f || upgradesNeeded < 0)
                return;

            string steamId = SemiFunc.PlayerGetSteamID(__instance.playerAvatar);
            int tumbleUpgrades = StatsManager.instance.playerUpgradeLaunch[steamId];

            float reductionRatio;
            if (upgradesNeeded == 0)
                reductionRatio = maxReduction;
            else
                reductionRatio = maxReduction * Math.Min((float)tumbleUpgrades / upgradesNeeded, 1f);

            float damageMultiplier = 1f - reductionRatio;

            int original = __instance.impactHurtDamage;
            int reduced = Math.Max((int)(original * damageMultiplier), 0);
            __instance.impactHurtDamage = reduced;

            Increase_Tumble_Damage.Logger.LogInfo(
                $"BreakImpact SCALED: impactHurtDamage {original} -> {reduced} (reduction: {reductionRatio:P0}, upgrades: {tumbleUpgrades})");
        }
        catch (Exception ex)
        {
            Increase_Tumble_Damage.Logger.LogError($"BreakImpact prefix exception: {ex.Message}");
        }
    }
}
