using EveryoneFights.Configuration;
using System;
using System.Threading;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EveryoneFights.Core
{
    public static class GenderOverrideManager
    {
        private static readonly ThreadLocal<bool> _overrideActive = new ThreadLocal<bool>(() => false);
        private static readonly ThreadLocal<bool> _overrideValue = new ThreadLocal<bool>(() => false);

        private static readonly string[] MaleOnlyTroopIds = new[]
        {
            "sturgian_warrior_son",
            "sturgian_heroic_line_breaker",
            "aserai_mameluke_soldier",
            "aserai_mameluke_heavy_cavalry",
            "aserai_mameluke_guard",
            "aserai_vanguard_faris",
            "nord_huscarl",
        };

        public static bool IsOverrideActive => _overrideActive.Value;
        public static bool OverrideIsFemale => _overrideValue.Value;

        public static void EnableOverride(BasicCharacterObject? character, int seed = 0)
        {
            _overrideActive.Value = true;
            _overrideValue.Value = ShouldBeFemale(character, seed);
        }

        public static void DisableOverride()
        {
            _overrideActive.Value = false;
            _overrideValue.Value = false;
        }

        public static bool ShouldBeFemale(BasicCharacterObject? character, int seed)
        {
            var settings = Settings.Instance;
            if (settings == null || !settings.Enabled)
                return false;
            if (character == null || character.IsFemale)
                return false;
            if (character.IsHero)
                return false;
            if (settings.LoreFriendly && IsMaleOnlyTroop(character))
                return false;

            Random rand = seed != 0 ? new Random(seed) : new Random();
            return rand.Next(100) < settings.FemalePercentage;
        }

        private static bool IsMaleOnlyTroop(BasicCharacterObject character)
        {
            if (character.StringId == null) return false;
            string id = character.StringId.ToLowerInvariant();
            foreach (var maleOnlyId in MaleOnlyTroopIds)
            {
                if (id.Contains(maleOnlyId)) return true;
            }
            return false;
        }

        public static int GenerateSeed(string? characterId, int contextSeed = 0)
        {
            if (string.IsNullOrEmpty(characterId)) return contextSeed;
            unchecked
            {
                int hash = characterId!.GetHashCode();
                return hash ^ (contextSeed * 397);
            }
        }
    }
}
