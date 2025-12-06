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
        private static void Postfix(ref bool __result)
        {
            if (GenderOverrideManager.IsOverrideActive)
            {
                __result = GenderOverrideManager.OverrideIsFemale;
            }
        }
    }
}
