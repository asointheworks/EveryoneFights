using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace EveryoneFights.Configuration
{
    internal sealed class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "EveryoneFights";
        public override string DisplayName => "Everyone Fights";
        public override string FolderName => "EveryoneFights";
        public override string FormatType => "json";

        [SettingPropertyBool("Enable Gender Diversity", Order = 0, RequireRestart = false,
            HintText = "Enable or disable female troops in battles and menus.")]
        [SettingPropertyGroup("General")]
        public bool Enabled { get; set; } = true;

        [SettingPropertyInteger("Female Troop Percentage", 0, 100, Order = 1, RequireRestart = false,
            HintText = "Percentage chance for a troop to appear as female (0-100).")]
        [SettingPropertyGroup("General")]
        public int FemalePercentage { get; set; } = 50;

        [SettingPropertyBool("Lore-Friendly Exceptions", Order = 2, RequireRestart = false,
            HintText = "Keep Skolderbroda and Ghilman as male-only (lore accurate).")]
        [SettingPropertyGroup("General")]
        public bool LoreFriendly { get; set; } = true;
    }
}
