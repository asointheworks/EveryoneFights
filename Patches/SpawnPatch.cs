using EveryoneFights.Core;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EveryoneFights.Patches
{
    [HarmonyPatch]
    internal static class SpawnPatch
    {
        [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
        [HarmonyPrefix]
        private static void SpawnAgentPrefix(AgentBuildData agentBuildData)
        {
            if (agentBuildData?.AgentCharacter == null)
                return;

            int seed = agentBuildData.AgentOrigin?.Seed ?? 0;
            GenderOverrideManager.EnableOverride(agentBuildData.AgentCharacter, seed);
        }

        [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
        [HarmonyPostfix]
        private static void SpawnAgentPostfix()
        {
            GenderOverrideManager.DisableOverride();
        }
    }

    [HarmonyPatch]
    internal static class IsFemaleGetterPatch
    {
        [HarmonyPatch(typeof(BasicCharacterObject), nameof(BasicCharacterObject.IsFemale), MethodType.Getter)]
        [HarmonyPostfix]
        private static void Postfix(BasicCharacterObject __instance, ref bool __result)
        {
            // Only change the result if:
            // 1. We're actively overriding
            // 2. This is the specific character we're overriding (not some other character queried during the operation)
            // 3. The override value is female
            // This prevents townspeople and other NPCs from being affected when spawning military troops
            if (GenderOverrideManager.IsOverrideActive &&
                GenderOverrideManager.IsTargetCharacter(__instance) &&
                GenderOverrideManager.OverrideIsFemale)
            {
                __result = true;
            }
        }
    }
}
