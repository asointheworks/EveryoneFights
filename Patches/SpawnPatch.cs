using GenderDiversity.Core;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace GenderDiversity.Patches
{
    /// <summary>
    /// Patches for battle/mission agent spawning.
    /// Handles gender override during Mission.SpawnAgent calls.
    /// </summary>
    [HarmonyPatch]
    internal static class SpawnPatch
    {
        /// <summary>
        /// Prefix: Enable gender override before agent spawns.
        /// </summary>
        [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
        [HarmonyPrefix]
        private static void SpawnAgentPrefix(AgentBuildData agentBuildData)
        {
            if (agentBuildData?.AgentCharacter == null)
                return;

            // Use the agent's seed for consistency (same troop = same gender in battle)
            int seed = agentBuildData.AgentOrigin?.Seed ?? 0;
            GenderOverrideManager.EnableOverride(agentBuildData.AgentCharacter, seed);
        }

        /// <summary>
        /// Postfix: Disable gender override after agent spawns.
        /// </summary>
        [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
        [HarmonyPostfix]
        private static void SpawnAgentPostfix()
        {
            GenderOverrideManager.DisableOverride();
        }
    }

    /// <summary>
    /// Patch for BasicCharacterObject.IsFemale getter.
    /// Returns overridden value when override is active.
    /// </summary>
    [HarmonyPatch]
    internal static class IsFemaleGetterPatch
    {
        [HarmonyPatch(typeof(BasicCharacterObject), nameof(BasicCharacterObject.IsFemale), MethodType.Getter)]
        [HarmonyPostfix]
        private static void Postfix(ref bool __result)
        {
            if (GenderOverrideManager.IsOverrideActive)
            {
                __result = GenderOverrideManager.OverrideIsFemale;
            }
        }
    }
}
