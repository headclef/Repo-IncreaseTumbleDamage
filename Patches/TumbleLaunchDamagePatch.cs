using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Increase_Tumble_Damage.Patches;

[HarmonyPatch]
internal static class TumbleLaunchDamagePatch
{
    /// <summary>
    /// Patches PlayerTumble.HitEnemy to replace the hardcoded self-damage value (5)
    /// with the configured value, if non-zero.
    /// </summary>
    [HarmonyPatch(typeof(PlayerTumble), nameof(PlayerTumble.HitEnemy))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HitEnemy_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        int configuredDamage = Increase_Tumble_Damage.TumbleDamageOnHitEnemy.Value;

        // If configured to 0, leave the original damage intact.
        if (configuredDamage == 0)
            return instructions;

        try
        {
            var matcher = new CodeMatcher(instructions);

            // Find the first occurrence of the hardcoded damage value (Ldc_I4_5).
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldc_I4_5));

            if (!matcher.IsValid)
            {
                Increase_Tumble_Damage.Logger.LogError("HitEnemy transpiler: failed to find first damage instruction.");
                Increase_Tumble_Damage.PatchFailed = true;
                return instructions;
            }

            matcher.Set(OpCodes.Ldc_I4, configuredDamage);

            // Find the second occurrence.
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldc_I4_5));

            if (!matcher.IsValid)
            {
                Increase_Tumble_Damage.Logger.LogError("HitEnemy transpiler: failed to find second damage instruction.");
                Increase_Tumble_Damage.PatchFailed = true;
                return instructions;
            }

            matcher.Set(OpCodes.Ldc_I4, configuredDamage);

            Increase_Tumble_Damage.Logger.LogDebug($"HitEnemy damage patched to {configuredDamage}.");
            return matcher.InstructionEnumeration();
        }
        catch (Exception ex)
        {
            Increase_Tumble_Damage.Logger.LogError($"HitEnemy transpiler exception: {ex.Message}");
            Increase_Tumble_Damage.PatchFailed = true;
            return instructions;
        }
    }

    /// <summary>
    /// Patches PlayerTumble.BreakImpact to inject a damage reduction calculation
    /// based on the player's Tumble Launch upgrade level.
    /// </summary>
    [HarmonyPatch(typeof(PlayerTumble), nameof(PlayerTumble.BreakImpact))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> BreakImpact_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            var matcher = new CodeMatcher(instructions);

            // Find the damage application: playerAvatar.playerHealth.Hurt(impactHurtDamage, ...)
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerTumble), nameof(PlayerTumble.playerAvatar))),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerAvatar), nameof(PlayerAvatar.playerHealth))),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerTumble), "impactHurtDamage")));

            if (!matcher.IsValid)
            {
                Increase_Tumble_Damage.Logger.LogError("BreakImpact transpiler: failed to find damage instruction.");
                Increase_Tumble_Damage.PatchFailed = true;
                return instructions;
            }

            // Advance past the matched instructions, then insert our reduction call.
            matcher.Advance(1);

            // Insert: load `this` (PlayerTumble) and call our reduction method.
            // Stack state at this point: [..., int damage]
            // After insert: [..., int reducedDamage]
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TumbleLaunchDamagePatch), nameof(CalculateReducedDamage))));

            Increase_Tumble_Damage.Logger.LogDebug("BreakImpact damage reduction patch applied.");
            return matcher.InstructionEnumeration();
        }
        catch (Exception ex)
        {
            Increase_Tumble_Damage.Logger.LogError($"BreakImpact transpiler exception: {ex.Message}");
            Increase_Tumble_Damage.PatchFailed = true;
            return instructions;
        }
    }

    /// <summary>
    /// Calculates reduced tumble impact damage based on the player's Tumble Launch upgrade level.
    /// Called from the IL-injected code in BreakImpact.
    /// </summary>
    public static int CalculateReducedDamage(int originalDamage, PlayerTumble tumble)
    {
        try
        {
            int upgradesNeeded = Increase_Tumble_Damage.UpgradesNeededForMaxReduction.Value;
            float maxReduction = Increase_Tumble_Damage.MaxDamageReduction.Value;

            // If max reduction is 0 or upgrades needed is negative, no scaling applies.
            if (maxReduction <= 0f || upgradesNeeded < 0)
                return originalDamage;

            // Instantly at max reduction if upgrades needed is 0.
            if (upgradesNeeded == 0)
            {
                return (int)((1f - maxReduction) * originalDamage);
            }

            // Read the player's current Tumble Launch upgrade count.
            string steamId = SemiFunc.PlayerGetSteamID(tumble.playerAvatar);
            int tumbleUpgrades = StatsManager.instance.playerUpgradeLaunch[steamId];

            // Calculate the reduction ratio, clamped to [0, maxReduction].
            float reductionRatio = maxReduction * Math.Min((float)tumbleUpgrades / upgradesNeeded, 1f);
            float damageMultiplier = 1f - reductionRatio;

            int reducedDamage = (int)(damageMultiplier * originalDamage);

            Increase_Tumble_Damage.Logger.LogDebug(
                $"Tumble damage: {originalDamage} -> {reducedDamage} (upgrades: {tumbleUpgrades}, reduction: {reductionRatio:P0})");

            return reducedDamage;
        }
        catch (Exception ex)
        {
            Increase_Tumble_Damage.Logger.LogError($"CalculateReducedDamage exception: {ex.Message}");
            return originalDamage;
        }
    }
}
