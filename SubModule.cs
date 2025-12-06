using EveryoneFights.Patches;
using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.MountAndBlade;

namespace EveryoneFights
{
    public class SubModule : MBSubModuleBase
    {
        private Harmony? _harmony;
        private static readonly string LogFile = "/tmp/EveryoneFights.log";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            
            try
            {
                File.WriteAllText(LogFile, $"[{DateTime.Now}] EveryoneFights loading...\n");
                
                _harmony = new Harmony("mod.everyonefights");
                File.AppendAllText(LogFile, $"[{DateTime.Now}] Harmony created\n");
                
                // Apply attribute-based patches (SpawnPatch, IsFemaleGetterPatch)
                _harmony.PatchAll();
                File.AppendAllText(LogFile, $"[{DateTime.Now}] PatchAll completed\n");
                
                // Apply manual ViewModel patches with logging
                ViewModelPatches.ApplyPatches(_harmony);
                File.AppendAllText(LogFile, $"[{DateTime.Now}] ViewModelPatches.ApplyPatches completed\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(LogFile, $"[{DateTime.Now}] ERROR: {ex}\n");
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            _harmony?.UnpatchAll("mod.everyonefights");
            base.OnSubModuleUnloaded();
        }
    }
}
