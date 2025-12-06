using EveryoneFights.Core;
using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.CampaignSystem;

namespace EveryoneFights.Patches
{
    public static class ViewModelPatches
    {
        private static Harmony? _harmony;
        // Use temp folder for cross-platform compatibility
        private static readonly string LogPath = Path.Combine(
            Path.GetTempPath(),
            "EveryoneFights.log"
        );

        public static void ApplyPatches(Harmony harmony)
        {
            _harmony = harmony;
            
            Log("=== EveryoneFights Patch Application Started ===");

            TryPatchPartyCharacterVM();
            TryPatchEncyclopediaUnitVM();
            TryPatchRecruitVolunteerTroopVM();
            
            Log("=== Patch Application Complete ===");
        }

        #region Party Screen

        private static void TryPatchPartyCharacterVM()
        {
            try
            {
                var type = AccessTools.TypeByName("TaleWorlds.CampaignSystem.ViewModelCollection.PartyCharacterVM");
                if (type == null)
                {
                    Log("ERROR: PartyCharacterVM type not found");
                    return;
                }
                Log($"Found PartyCharacterVM: {type.FullName}");

                var characterSetter = AccessTools.PropertySetter(type, "Character");
                if (characterSetter != null)
                {
                    _harmony?.Patch(characterSetter,
                        prefix: new HarmonyMethod(typeof(ViewModelPatches), nameof(PartyCharacterVM_Character_Prefix)),
                        postfix: new HarmonyMethod(typeof(ViewModelPatches), nameof(Generic_Postfix)));
                    Log("  Patched Character setter");
                }
                else
                {
                    Log("  WARNING: Character setter not found");
                }

                var troopSetter = AccessTools.PropertySetter(type, "Troop");
                if (troopSetter != null)
                {
                    _harmony?.Patch(troopSetter,
                        prefix: new HarmonyMethod(typeof(ViewModelPatches), nameof(PartyCharacterVM_Troop_Prefix)),
                        postfix: new HarmonyMethod(typeof(ViewModelPatches), nameof(Generic_Postfix)));
                    Log("  Patched Troop setter");
                }
                else
                {
                    Log("  WARNING: Troop setter not found");
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR patching PartyCharacterVM: {ex}");
            }
        }

        public static void PartyCharacterVM_Character_Prefix(CharacterObject value, object __instance)
        {
            if (value == null || value.IsHero)
                return;

            int seed = GenderOverrideManager.GenerateSeed(value.StringId, __instance?.GetHashCode() ?? 0);
            GenderOverrideManager.EnableOverride(value, seed);
        }

        public static void PartyCharacterVM_Troop_Prefix(object value, object __instance)
        {
            if (value == null) return;
            
            var characterProp = value.GetType().GetProperty("Character");
            if (characterProp == null) return;
            
            var character = characterProp.GetValue(value) as CharacterObject;
            if (character == null || character.IsHero)
                return;

            int seed = GenderOverrideManager.GenerateSeed(character.StringId, __instance?.GetHashCode() ?? 0);
            GenderOverrideManager.EnableOverride(character, seed);
        }

        #endregion

        #region Encyclopedia

        private static void TryPatchEncyclopediaUnitVM()
        {
            try
            {
                var type = AccessTools.TypeByName("TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items.EncyclopediaUnitVM");
                if (type == null)
                {
                    Log("ERROR: EncyclopediaUnitVM type not found");
                    return;
                }
                Log($"Found EncyclopediaUnitVM: {type.FullName}");

                // List all constructors
                var ctors = type.GetConstructors();
                Log($"  Found {ctors.Length} constructor(s)");
                foreach (var c in ctors)
                {
                    var ps = c.GetParameters();
                    Log($"    ctor({string.Join(", ", Array.ConvertAll(ps, p => p.ParameterType.Name))})");
                }

                var ctor = AccessTools.Constructor(type, new Type[] { typeof(CharacterObject), typeof(bool) });
                if (ctor != null)
                {
                    _harmony?.Patch(ctor,
                        prefix: new HarmonyMethod(typeof(ViewModelPatches), nameof(EncyclopediaUnitVM_Prefix)),
                        postfix: new HarmonyMethod(typeof(ViewModelPatches), nameof(Generic_Postfix)));
                    Log("  Patched constructor (CharacterObject, bool)");
                }
                else
                {
                    Log("  WARNING: Constructor (CharacterObject, bool) not found");
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR patching EncyclopediaUnitVM: {ex}");
            }
        }

        public static void EncyclopediaUnitVM_Prefix(CharacterObject character)
        {
            if (character == null || character.IsHero)
                return;

            int seed = character.StringId?.GetHashCode() ?? 0;
            GenderOverrideManager.EnableOverride(character, seed);
        }

        #endregion

        #region Recruitment

        private static void TryPatchRecruitVolunteerTroopVM()
        {
            try
            {
                var type = AccessTools.TypeByName("TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.RecruitVolunteerTroopVM");
                if (type == null)
                {
                    Log("ERROR: RecruitVolunteerTroopVM type not found");
                    return;
                }
                Log($"Found RecruitVolunteerTroopVM: {type.FullName}");

                var ctors = type.GetConstructors();
                Log($"  Found {ctors.Length} constructor(s)");
                
                bool patched = false;
                foreach (var ctor in ctors)
                {
                    var parameters = ctor.GetParameters();
                    Log($"    ctor({string.Join(", ", Array.ConvertAll(parameters, p => p.ParameterType.Name))})");
                    
                    // Look for CharacterObject parameter
                    bool hasCharacter = false;
                    foreach (var p in parameters)
                    {
                        if (p.ParameterType == typeof(CharacterObject))
                        {
                            hasCharacter = true;
                            break;
                        }
                    }

                    if (hasCharacter && !patched)
                    {
                        _harmony?.Patch(ctor,
                            prefix: new HarmonyMethod(typeof(ViewModelPatches), nameof(RecruitVolunteerTroopVM_Prefix)),
                            postfix: new HarmonyMethod(typeof(ViewModelPatches), nameof(Generic_Postfix)));
                        Log("    ^ Patched this constructor");
                        patched = true;
                    }
                }
                
                if (!patched)
                {
                    Log("  WARNING: No suitable constructor found to patch");
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR patching RecruitVolunteerTroopVM: {ex}");
            }
        }

        public static void RecruitVolunteerTroopVM_Prefix(object[] __args)
        {
            CharacterObject? character = null;
            int index = 0;
            
            if (__args != null)
            {
                for (int i = 0; i < __args.Length; i++)
                {
                    if (__args[i] is CharacterObject c)
                        character = c;
                    else if (__args[i] is int idx)
                        index = idx;
                }
            }
            
            if (character == null || character.IsHero)
                return;

            int seed = GenderOverrideManager.GenerateSeed(character.StringId, index);
            GenderOverrideManager.EnableOverride(character, seed);
        }

        #endregion

        #region Common

        public static void Generic_Postfix()
        {
            GenderOverrideManager.DisableOverride();
        }

        private static void Log(string message)
        {
            try
            {
                var dir = Path.GetDirectoryName(LogPath);
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                    
                File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
            }
            catch
            {
                // Ignore logging errors
            }
        }

        #endregion
    }
}
