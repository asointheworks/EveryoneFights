using GenderDiversity.Core;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;

namespace GenderDiversity.Patches
{
    /// <summary>
    /// Manual patches for ViewModel classes.
    /// Uses runtime type resolution to avoid compile-time assembly dependency issues.
    /// </summary>
    public static class ViewModelPatches
    {
        private static Harmony _harmony;

        /// <summary>
        /// Apply all ViewModel patches manually.
        /// Call this from SubModule.OnSubModuleLoad after harmony.PatchAll()
        /// </summary>
        public static void ApplyPatches(Harmony harmony)
        {
            _harmony = harmony;

            TryPatchPartyCharacterVM();
            TryPatchEncyclopediaUnitVM();
            TryPatchRecruitVolunteerTroopVM();
        }

        #region Party Screen

        private static void TryPatchPartyCharacterVM()
        {
            try
            {
                var type = AccessTools.TypeByName("TaleWorlds.CampaignSystem.ViewModelCollection.PartyCharacterVM");
                if (type == null)
                {
                    LogWarning("PartyCharacterVM type not found");
                    return;
                }

                // Patch the Character property setter
                var characterSetter = AccessTools.PropertySetter(type, "Character");
                if (characterSetter != null)
                {
                    _harmony.Patch(characterSetter,
                        prefix: new HarmonyMethod(typeof(ViewModelPatches), nameof(PartyCharacterVM_Character_Prefix)),
                        postfix: new HarmonyMethod(typeof(ViewModelPatches), nameof(Generic_Postfix)));
                }

                // Patch the Troop property setter
                var troopSetter = AccessTools.PropertySetter(type, "Troop");
                if (troopSetter != null)
                {
                    _harmony.Patch(troopSetter,
                        prefix: new HarmonyMethod(typeof(ViewModelPatches), nameof(PartyCharacterVM_Troop_Prefix)),
                        postfix: new HarmonyMethod(typeof(ViewModelPatches), nameof(Generic_Postfix)));
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to patch PartyCharacterVM: {ex.Message}");
            }
        }

        public static void PartyCharacterVM_Character_Prefix(CharacterObject value, object __instance)
        {
            if (value == null || value.IsHero)
                return;

            int seed = GenderOverrideManager.GenerateSeed(
                value.StringId,
                __instance?.GetHashCode() ?? 0
            );
            GenderOverrideManager.EnableOverride(value, seed);
        }

        public static void PartyCharacterVM_Troop_Prefix(object value, object __instance)
        {
            // TroopRosterElement is a struct - get Character property via reflection
            if (value == null) return;
            
            var characterProp = value.GetType().GetProperty("Character");
            if (characterProp == null) return;
            
            var character = characterProp.GetValue(value) as CharacterObject;
            if (character == null || character.IsHero)
                return;

            int seed = GenderOverrideManager.GenerateSeed(
                character.StringId,
                __instance?.GetHashCode() ?? 0
            );
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
                    LogWarning("EncyclopediaUnitVM type not found");
                    return;
                }

                // Find constructor that takes (CharacterObject, bool)
                var ctor = AccessTools.Constructor(type, new Type[] { typeof(CharacterObject), typeof(bool) });
                if (ctor != null)
                {
                    _harmony.Patch(ctor,
                        prefix: new HarmonyMethod(typeof(ViewModelPatches), nameof(EncyclopediaUnitVM_Prefix)),
                        postfix: new HarmonyMethod(typeof(ViewModelPatches), nameof(Generic_Postfix)));
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to patch EncyclopediaUnitVM: {ex.Message}");
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
                    LogWarning("RecruitVolunteerTroopVM type not found");
                    return;
                }

                // Find constructors - signature varies by version
                // Try common signatures
                foreach (var ctor in type.GetConstructors())
                {
                    var parameters = ctor.GetParameters();
                    // Look for constructor that has CharacterObject parameter
                    int charIndex = -1;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType == typeof(CharacterObject))
                        {
                            charIndex = i;
                            break;
                        }
                    }

                    if (charIndex >= 0)
                    {
                        _harmony.Patch(ctor,
                            prefix: new HarmonyMethod(typeof(ViewModelPatches), nameof(RecruitVolunteerTroopVM_Prefix)),
                            postfix: new HarmonyMethod(typeof(ViewModelPatches), nameof(Generic_Postfix)));
                        break; // Only patch one constructor
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to patch RecruitVolunteerTroopVM: {ex.Message}");
            }
        }

        public static void RecruitVolunteerTroopVM_Prefix(object[] __args)
        {
            // Constructor parameters vary by version
            // Look for CharacterObject in the arguments
            CharacterObject character = null;
            int index = 0;
            
            for (int i = 0; i < __args.Length; i++)
            {
                if (__args[i] is CharacterObject c)
                {
                    character = c;
                }
                else if (__args[i] is int idx)
                {
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

        private static void LogWarning(string message)
        {
            // Using TaleWorlds logging if available, otherwise console
            System.Diagnostics.Debug.WriteLine($"[GenderDiversity] {message}");
        }

        #endregion
    }
}
