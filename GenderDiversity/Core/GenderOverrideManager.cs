using GenderDiversity.Configuration;
using System;
using System.Threading;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace GenderDiversity.Core
{
    /// <summary>
    /// Centralized manager for gender override logic.
    /// Provides thread-safe enable/disable pattern for Harmony patches.
    /// </summary>
    public static class GenderOverrideManager
    {
        // Thread-local to handle potential concurrent access
        private static readonly ThreadLocal<bool> _overrideActive = new ThreadLocal<bool>(() => false);
        private static readonly ThreadLocal<bool> _overrideValue = new ThreadLocal<bool>(() => false);

        // Male-only troops for lore-friendly mode
        private static readonly string[] MaleOnlyTroopIds = new[]
        {
            // Skolderbroda (Sturgian warrior sons)
            "sturgian_warrior_son",
            "sturgian_heroic_line_breaker",
            // Ghilman (Aserai slave soldiers)
            "aserai_mameluke_soldier",
            "aserai_mameluke_heavy_cavalry",
            "aserai_mameluke_guard",
            "aserai_vanguard_faris",
            // Nord Huscarls (if using mods)
            "nord_huscarl",
        };

        /// <summary>
        /// Check if gender override is currently active.
        /// </summary>
        public static bool IsOverrideActive => _overrideActive.Value;

        /// <summary>
        /// Get the current override value (true = female).
        /// </summary>
        public static bool OverrideIsFemale => _overrideValue.Value;

        /// <summary>
        /// Enable gender override for a character. Call before VM creates visual.
        /// </summary>
        /// <param name="character">The character to potentially override</param>
        /// <param name="seed">Seed for deterministic randomization (0 for random)</param>
        public static void EnableOverride(BasicCharacterObject character, int seed = 0)
        {
            _overrideActive.Value = true;
            _overrideValue.Value = ShouldBeFemale(character, seed);
        }

        /// <summary>
        /// Enable override for CharacterObject with automatic seed from StringId.
        /// </summary>
        public static void EnableOverride(CharacterObject character)
        {
            int seed = character?.StringId?.GetHashCode() ?? 0;
            EnableOverride(character, seed);
        }

        /// <summary>
        /// Disable gender override. Call in postfix after VM finishes.
        /// </summary>
        public static void DisableOverride()
        {
            _overrideActive.Value = false;
            _overrideValue.Value = false;
        }

        /// <summary>
        /// Determine if a character should be rendered as female.
        /// </summary>
        /// <param name="character">The character to check</param>
        /// <param name="seed">Seed for deterministic random (0 = use shared random)</param>
        /// <returns>True if character should be female</returns>
        public static bool ShouldBeFemale(BasicCharacterObject character, int seed)
        {
            var settings = Settings.Instance;
            
            // Check if mod is enabled
            if (settings == null || !settings.Enabled)
                return false;

            // Don't change already-female characters
            if (character == null || character.IsFemale)
                return false;

            // Don't change heroes (named characters)
            if (character.IsHero)
                return false;

            // Check lore-friendly exceptions
            if (settings.LoreFriendly && IsMaleOnlyTroop(character))
                return false;

            // Deterministic random based on seed
            Random rand = seed != 0 ? new Random(seed) : new Random();
            return rand.Next(100) < settings.FemalePercentage;
        }

        /// <summary>
        /// Check if a troop is in the male-only list (lore exceptions).
        /// </summary>
        private static bool IsMaleOnlyTroop(BasicCharacterObject character)
        {
            if (character?.StringId == null)
                return false;

            string id = character.StringId.ToLowerInvariant();
            foreach (var maleOnlyId in MaleOnlyTroopIds)
            {
                if (id.Contains(maleOnlyId))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Generate a consistent seed for a character in a specific context.
        /// Combines character identity with context for variety while maintaining consistency.
        /// </summary>
        /// <param name="characterId">Character StringId</param>
        /// <param name="contextSeed">Additional context (e.g., party index, roster position)</param>
        public static int GenerateSeed(string characterId, int contextSeed = 0)
        {
            if (string.IsNullOrEmpty(characterId))
                return contextSeed;
            
            unchecked
            {
                int hash = characterId.GetHashCode();
                return hash ^ (contextSeed * 397);
            }
        }
    }
}
