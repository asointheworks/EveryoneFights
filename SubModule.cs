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
            _harmony.PatchAll();
        }

        protected override void OnSubModuleUnloaded()
        {
            _harmony?.UnpatchAll("mod.genderdiversity");
            base.OnSubModuleUnloaded();
        }
    }
}
