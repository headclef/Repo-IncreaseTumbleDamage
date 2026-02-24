using System;
using static Character_Stats.Character_Stats;
using HarmonyLib;
using UnityEngine;

namespace Increase_Tumble_Damage.Patches;

[HarmonyPatch]
internal static class TumbleLaunchDamagePatch
{
    // ── Helper MonoBehaviour to remember the original base damage ──
    public class BaseDamageTag : MonoBehaviour
    {
        public int baseDamage;
    }

    // ── Enemy Damage Scaling ──

    [HarmonyPatch(typeof(HurtCollider), "EnemyHurt")]
    [HarmonyPrefix]
    private static void EnemyHurt_Prefix(HurtCollider __instance)
    {
        if (!Increase_Tumble_Damage.EnableDamageOnEnemy.Value)
            return;

        try
        {
            // The tumble HurtCollider lives under: PlayerAvatar(Clone) / Player Tumble(Clone) / Hurt Collider
            // GetComponentInParent<PlayerTumble>() will find it only for the player's tumble collider.
            var playerTumble = __instance.GetComponentInParent<PlayerTumble>();
            if (playerTumble == null)
                return; // Not the player's tumble collider (weapon, explosion, enemy, etc.)

            // Save the original base damage so we can restore it in the postfix
            // (otherwise the damage would compound on repeated hits)
            if (!__instance.TryGetComponent<BaseDamageTag>(out var tag))
            {
                tag = __instance.gameObject.AddComponent<BaseDamageTag>();
                tag.baseDamage = __instance.enemyDamage;
            }

            // Get upgrade level from the player who owns this tumble collider
            var avatar = playerTumble.playerAvatar;
            if (avatar == null)
                return;

            string steamId = SemiFunc.PlayerGetSteamID(avatar);
            int tumbleUpgrades = GetUpgradeLevel(steamId, "Launch");

            float multiplierPerLevel = Increase_Tumble_Damage.MultiplierPerLevel.Value;
            float maxMultiplier = Increase_Tumble_Damage.MaxMultiplier.Value;

            if (multiplierPerLevel <= 0f || tumbleUpgrades <= 0)
                return;

            // Formula: multiplier = level × MultiplierPerLevel
            float multiplier = multiplierPerLevel * tumbleUpgrades;
            if (maxMultiplier > 0f)
                multiplier = Math.Min(multiplier, maxMultiplier);

            // Scale the enemyDamage field directly (same approach as reference mod)
            __instance.enemyDamage = Mathf.RoundToInt(tag.baseDamage * multiplier);

            Increase_Tumble_Damage.Logger.LogInfo(
                $"EnemyHurt TUMBLE: damage {tag.baseDamage} -> {__instance.enemyDamage} (multiplier: {multiplier:F2}, upgrades: {tumbleUpgrades})");
        }
        catch (Exception ex)
        {
            Increase_Tumble_Damage.Logger.LogError($"EnemyHurt prefix exception: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(HurtCollider), "EnemyHurt")]
    [HarmonyPostfix]
    private static void EnemyHurt_Postfix(HurtCollider __instance)
    {
        // Restore original damage after hit so it doesn't compound
        if (__instance.TryGetComponent<BaseDamageTag>(out var tag))
        {
            __instance.enemyDamage = tag.baseDamage;
        }
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
            int tumbleUpgrades = GetUpgradeLevel(steamId, "Launch");

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
