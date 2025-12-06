using GenderDiversity.Core;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;

namespace GenderDiversity.Patches
{
    /// <summary>
    /// Patches for ViewModel classes that display troop portraits.
    /// Enables gender override before character visuals are created.
    /// 
    /// WARNING: ViewModel signatures may change between game versions.
    /// These patches target 1.2.x/1.3.x. If crashes occur after game updates,
    /// check if constructor/method signatures have changed.
    /// </summary>
    [HarmonyPatch]
    internal static class ViewModelPatches
    {
        #region Party Screen Patches

        /// <summary>
        /// Patch PartyCharacterVM.Character setter.
        /// This is called when setting the character for display in party screen.
        /// </summary>
        [HarmonyPatch(typeof(PartyCharacterVM), nameof(PartyCharacterVM.Character), MethodType.Setter)]
        internal static class PartyCharacterVM_Character_Patch
        {
            static void Prefix(CharacterObject value, PartyCharacterVM __instance)
            {
                if (value == null || value.IsHero)
                    return;

                // Generate seed from character ID and some instance context
                // Using the instance's hashcode adds variety per roster slot
                int seed = GenderOverrideManager.GenerateSeed(
                    value.StringId, 
                    __instance?.GetHashCode() ?? 0
                );
                GenderOverrideManager.EnableOverride(value, seed);
            }

            static void Postfix()
            {
                GenderOverrideManager.DisableOverride();
            }
        }

        /// <summary>
        /// Alternative: Patch PartyCharacterVM.Troop setter if Character patch doesn't work.
        /// The Troop property wraps TroopRosterElement which contains the CharacterObject.
        /// </summary>
        [HarmonyPatch(typeof(PartyCharacterVM), nameof(PartyCharacterVM.Troop), MethodType.Setter)]
        internal static class PartyCharacterVM_Troop_Patch
        {
            static void Prefix(TroopRosterElement value, PartyCharacterVM __instance)
            {
                var character = value.Character;
                if (character == null || character.IsHero)
                    return;

                int seed = GenderOverrideManager.GenerateSeed(
                    character.StringId,
                    __instance?.GetHashCode() ?? 0
                );
                GenderOverrideManager.EnableOverride(character, seed);
            }

            static void Postfix()
            {
                GenderOverrideManager.DisableOverride();
            }
        }

        #endregion

        #region Recruitment Screen Patches

        /// <summary>
        /// Patch RecruitVolunteerTroopVM constructor.
        /// Called when creating volunteer troop display in recruitment menu.
        /// 
        /// Constructor signature (1.2.x):
        /// RecruitVolunteerTroopVM(RecruitVolunteerVM owner, CharacterObject character, 
        ///                         int index, Action<RecruitVolunteerTroopVM> onClick, 
        ///                         Action<RecruitVolunteerTroopVM> onRemoveFromCart)
        /// </summary>
        [HarmonyPatch(typeof(RecruitVolunteerTroopVM), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { 
            typeof(RecruitVolunteerVM), 
            typeof(CharacterObject), 
            typeof(int), 
            typeof(Action<RecruitVolunteerTroopVM>), 
            typeof(Action<RecruitVolunteerTroopVM>) 
        })]
        internal static class RecruitVolunteerTroopVM_Ctor_Patch
        {
            static void Prefix(CharacterObject character, int index)
            {
                if (character == null || character.IsHero)
                    return;

                // Use index for variety between volunteer slots
                int seed = GenderOverrideManager.GenerateSeed(character.StringId, index);
                GenderOverrideManager.EnableOverride(character, seed);
            }

            static void Postfix()
            {
                GenderOverrideManager.DisableOverride();
            }
        }

        #endregion

        #region Encyclopedia Patches

        /// <summary>
        /// Patch EncyclopediaUnitVM constructor.
        /// Called when displaying troop in encyclopedia.
        /// 
        /// Note: Encyclopedia shows the "canonical" troop, so we use a fixed seed
        /// based only on the character ID for consistency.
        /// </summary>
        [HarmonyPatch(typeof(EncyclopediaUnitVM), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(CharacterObject), typeof(bool) })]
        internal static class EncyclopediaUnitVM_Ctor_Patch
        {
            static void Prefix(CharacterObject character)
            {
                if (character == null || character.IsHero)
                    return;

                // Fixed seed for encyclopedia - same troop always shows same gender
                int seed = character.StringId?.GetHashCode() ?? 0;
                GenderOverrideManager.EnableOverride(character, seed);
            }

            static void Postfix()
            {
                GenderOverrideManager.DisableOverride();
            }
        }

        /// <summary>
        /// Patch EncyclopediaTroopTreeNodeVM constructor.
        /// Called when displaying troop tree in encyclopedia.
        /// </summary>
        [HarmonyPatch(typeof(EncyclopediaTroopTreeNodeVM), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { 
            typeof(CharacterObject), 
            typeof(CharacterObject), 
            typeof(bool),
            typeof(TaleWorlds.CampaignSystem.CharacterDevelopment.PerkObject)
        })]
        internal static class EncyclopediaTroopTreeNodeVM_Ctor_Patch
        {
            static void Prefix(CharacterObject rootCharacter)
            {
                if (rootCharacter == null || rootCharacter.IsHero)
                    return;

                int seed = rootCharacter.StringId?.GetHashCode() ?? 0;
                GenderOverrideManager.EnableOverride(rootCharacter, seed);
            }

            static void Postfix()
            {
                GenderOverrideManager.DisableOverride();
            }
        }

        #endregion
    }
}
