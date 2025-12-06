# Gender Diversity

A Mount & Blade II: Bannerlord mod that enables gender diversity in armies. Troops can appear as female based on configurable percentages, visible both in battles AND in UI menus (recruitment, party screen, encyclopedia).

## Features

- **Battle Diversity**: Troops spawn as male or female during battles based on configured percentage
- **UI Consistency**: Female troops appear correctly in:
  - Party screen (troop roster)
  - Recruitment menus (village/town volunteers)
  - Encyclopedia (troop pages and upgrade trees)
- **Configurable**: Set female percentage from 0-100% via MCM settings
- **Lore-Friendly Mode**: Optional setting to keep historically male-only units (Skolderbroda, Ghilman) as male
- **Consistent Rendering**: Same troop renders as same gender across all contexts using seed-based randomization

## Requirements

- Bannerlord 1.2.x or later (tested on 1.3.9)
- [Harmony](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006) 2.3.0+
- [ButterLib](https://www.nexusmods.com/mountandblade2bannerlord/mods/2018) 2.9.0+
- [Mod Configuration Menu (MCM)](https://www.nexusmods.com/mountandblade2bannerlord/mods/612) 5.10.0+

## Installation

1. Install the required dependencies (Harmony, ButterLib, MCM)
2. Download the latest release
3. Extract `GenderDiversity` folder to your `Modules` directory:
   - Windows: `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\`
   - Linux: `~/.steam/debian-installation/steamapps/common/Mount & Blade II Bannerlord/Modules/`
4. Enable the mod in the launcher (load after MCM and ButterLib)

## Configuration

Open MCM settings in-game to configure:

| Setting | Default | Description |
|---------|---------|-------------|
| Enable Gender Diversity | On | Master toggle for the mod |
| Female Troop Percentage | 50% | Chance for troops to appear female (0-100) |
| Lore-Friendly Exceptions | On | Keep Skolderbroda and Ghilman as male-only |

## How It Works

The mod uses Harmony to patch:

1. **Battle spawning** (`Mission.SpawnAgent`) - Changes gender during agent creation
2. **IsFemale property** (`BasicCharacterObject.IsFemale`) - Returns overridden value when active
3. **Party screen** (`PartyCharacterVM`) - Applies gender before portrait renders
4. **Recruitment** (`RecruitVolunteerTroopVM`) - Applies gender for volunteer display
5. **Encyclopedia** (`EncyclopediaUnitVM`, `EncyclopediaTroopTreeNodeVM`) - Applies gender for troop pages

Gender determination uses seeded randomization so the same troop renders consistently across different views.

## Compatibility

- Safe to install/uninstall mid-playthrough (no save data modified)
- Should work with most troop mods (uses generic character patching)
- May conflict with other mods that patch the same ViewModel classes

### Known Issues

- If game version updates change ViewModel constructor signatures, patches may fail. Report issues with your game version.

## Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/GenderDiversity.git
cd GenderDiversity

# Build (requires .NET SDK)
dotnet build --configuration Release

# Output: bin/Release/net472/GenderDiversity.dll
```

Or push to GitHub and let GitHub Actions build automatically.

## License

MIT License - feel free to use, modify, and distribute.

## Credits

- Inspired by [Female Troops Simplified](https://www.nexusmods.com/mountandblade2bannerlord/mods/5191) by woodbyte
- Built with [Harmony](https://github.com/pardeike/Harmony) by pardeike
- Uses [BUTR](https://github.com/BUTR) community tools (ButterLib, MCM)
