using EveryoneFights.Patches;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace EveryoneFights
{
    public class SubModule : MBSubModuleBase
    {
        private Harmony? _harmony;
        private static string? _logFile;

        private static string GetLogPath()
        {
            if (_logFile == null)
            {
                // Write log next to the DLL
                var dllPath = Assembly.GetExecutingAssembly().Location;
                var dllDir = Path.GetDirectoryName(dllPath) ?? "";
                _logFile = Path.Combine(dllDir, "EveryoneFights.log");
            }
            return _logFile;
        }

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(GetLogPath(), $"[{DateTime.Now:HH:mm:ss}] {message}\n");
            }
            catch { }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            
            try
            {
                File.WriteAllText(GetLogPath(), $"[{DateTime.Now:HH:mm:ss}] EveryoneFights loading...\n");
                
                _harmony = new Harmony("mod.everyonefights");
                Log("Harmony instance created");
                
                _harmony.PatchAll();
                Log("PatchAll completed (SpawnPatch, IsFemaleGetterPatch)");
                
                ViewModelPatches.ApplyPatches(_harmony);
                Log("ViewModelPatches.ApplyPatches completed");
            }
            catch (Exception ex)
            {
                Log($"ERROR in OnSubModuleLoad: {ex}");
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            _harmony?.UnpatchAll("mod.everyonefights");
            base.OnSubModuleUnloaded();
        }
    }
}
