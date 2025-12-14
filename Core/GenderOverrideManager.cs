using EveryoneFights.Configuration;
using System;
using System.Threading;
using TaleWorlds.Core;

namespace EveryoneFights.Core
{
    public static class GenderOverrideManager
    {
        private static readonly ThreadLocal<bool> _overrideActive = new ThreadLocal<bool>(() => false);
        private static readonly ThreadLocal<bool> _overrideValue = new ThreadLocal<bool>(() => false);

        // Lore-friendly exceptions: troops that should remain single-gender
        // 
        // Male-only troops - these will never become female:
        private static readonly string[] MaleOnlyTroopIds = new[]
        {
            // Skolderbroda (Nordic mercenary brotherhood based on Jomsvikings - male only by lore)
            // Troop IDs: skolderbrotva_tier_1, skolderbrotva_tier_2, skolderbrotva_tier_3
            "skolderbrotva",
            
            // Ghilman/Mamlukes (Aserai slave-soldiers - historically male only)
            // Troop IDs: aserai_mameluke_soldier, aserai_mameluke_regular, aserai_mameluke_heavy_cavalry, 
            //            aserai_mameluke_guard, aserai_vanguard_faris
            "aserai_mameluke",
            "vanguard_faris",
            
            // Ghulam variants (alternative Ghilman naming)
            "ghulam",
        };
        
        // Female-only troops - these are already female in game data, but we explicitly protect them
        // to ensure they're never accidentally affected
        private static readonly string[] FemaleOnlyTroopIds = new[]
        {
            // Sword Sisters mercenary line
            "sword_sister",
            "sword_follower", 
            "gallant_sword_sister",
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
            if (character == null)
                return false;
            
            // Already female - don't change (protects Sword Sisters, etc.)
            if (character.IsFemale)
                return false;
            
            // Heroes have their own gender
            if (character.IsHero)
                return false;
            
            // Don't affect civilians (townspeople, villagers, etc.)
            if (IsCivilian(character))
                return false;
            
            // Lore-friendly mode protections
            if (settings.LoreFriendly)
            {
                // Male-only troops stay male
                if (IsMaleOnlyTroop(character))
                    return false;
                
                // Female-only troops - extra safety check (they should already be IsFemale=true)
                if (IsFemaleOnlyTroop(character))
                    return false;
            }

            Random rand = seed != 0 ? new Random(seed) : new Random();
            return rand.Next(100) < settings.FemalePercentage;
        }

        private static bool IsCivilian(BasicCharacterObject character)
        {
            if (character.StringId == null) return false;
            string id = character.StringId.ToLowerInvariant();
            
            // Civilian StringId patterns - these should NOT be affected
            string[] civilianPatterns = new[]
            {
                // Town NPCs
                "townsman",
                "townswoman", 
                "town_",
                
                // Village NPCs  
                "villager",
                "village_",
                
                // Common civilians
                "peasant",
                "beggar",
                "child",
                
                // Merchants and shopkeepers
                "merchant",
                "shopworker",
                "shopkeeper",
                "barber",
                "blacksmith",
                "tavernkeeper",
                "tavern_wench",
                "innkeeper",
                
                // Entertainers
                "musician",
                "dancer",
                "gambler",
                
                // Arena staff
                "arena_master",
                "arena_",
                
                // Craftsmen and traders
                "weaponsmith",
                "armorer",
                "horse_merchant",
                "goods_merchant",
                "ransom_broker",
                "artisan",
                "craftsman",
                
                // Notables and leaders
                "notable",
                "gang_leader",
                "headman",
                "rural_notable",
                "sp_notable",
                "elder",
                
                // Generic civilian markers
                "_civilian",
                "_noncombatant",
                "_unarmed",
                
                // Caravan civilians (not guards)
                "caravan_master",
                
                // Prisoners/slaves (non-combat)
                "prisoner",
            };
            
            foreach (var pattern in civilianPatterns)
            {
                if (id.Contains(pattern))
                    return true;
            }
            return false;
        }

        private static bool IsMaleOnlyTroop(BasicCharacterObject character)
        {
            if (character.StringId == null) return false;
            string id = character.StringId.ToLowerInvariant();
            foreach (var maleOnlyId in MaleOnlyTroopIds)
            {
                if (id.Contains(maleOnlyId))
                    return true;
            }
            return false;
        }
        
        private static bool IsFemaleOnlyTroop(BasicCharacterObject character)
        {
            if (character.StringId == null) return false;
            string id = character.StringId.ToLowerInvariant();
            foreach (var femaleOnlyId in FemaleOnlyTroopIds)
            {
                if (id.Contains(femaleOnlyId))
                    return true;
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
