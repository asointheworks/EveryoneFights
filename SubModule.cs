using EveryoneFights.Patches;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace EveryoneFights
{
    public class SubModule : MBSubModuleBase
    {
        private Harmony? _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            
            _harmony = new Harmony("mod.everyonefights");
            
            // Apply attribute-based patches (SpawnPatch, IsFemaleGetterPatch)
            _harmony.PatchAll();
            
            // Apply manual ViewModel patches with logging
            ViewModelPatches.ApplyPatches(_harmony);
        }

        protected override void OnSubModuleUnloaded()
        {
            _harmony?.UnpatchAll("mod.everyonefights");
            base.OnSubModuleUnloaded();
        }
    }
}
