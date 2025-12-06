using GenderDiversity.Patches;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace GenderDiversity
{
    public class SubModule : MBSubModuleBase
    {
        private Harmony _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            
            _harmony = new Harmony("mod.genderdiversity");
            
            // Apply attribute-based patches (SpawnPatch, IsFemaleGetterPatch)
            _harmony.PatchAll();
            
            // Apply manual ViewModel patches (uses runtime type resolution)
            ViewModelPatches.ApplyPatches(_harmony);
        }

        protected override void OnSubModuleUnloaded()
        {
            _harmony?.UnpatchAll("mod.genderdiversity");
            base.OnSubModuleUnloaded();
        }
    }
}
