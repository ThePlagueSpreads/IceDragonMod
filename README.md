# Ice Dragon Mod
This is the official repository for the Ice Dragon Leviathan mod for Subnautica (and possibly Below Zero in the future)!!!

# Downloading the Mod

The latest release can be found in the [Releases](https://github.com/ThePlagueSpreads/IceDragonMod/releases) page or [on Nexus Mods](https://www.nexusmods.com/subnautica/mods/3550).

# Building the Mod
1. Clone or download the repository locally
2. Copy `IceDragon/TargetDirectories.targets.template`, rename it to `TargetDirectories.targets`, and ensure your game directory is correct.
3. Open the IceDragonAssets folder in Unity 2019.4.36f1 (or the security hotfix version).
4. In Unity, open the AssetBundle Browser, set your Build Target (e.g., Standalone Windows 64 or Standalone OSX Universal), and BUILD
5. Wait for the assets to fully compile so the C# project can automatically copy them.
6. Open the .sln file in your preferred IDE (e.g., Rider or Visual Studio).
7. Ensure all dependencies are resolved and build the solution.

 
If everything worked correctly, various files should have been copied to the folder at `...\Subnautica\BepInEx\plugins\IceDragon\`

> [!WARNING]
> All assets in this repository belong to their respective creators and are legally protected from reuse unless otherwise specified.

# Credits

- alvstxr: Sound design & music
- Beast: Programming, writing, and 2D art
- Kallie23: Programming, design, animation & 3D art
- NoContextCass: Modeling, texturing, and level design
- Rupter: Rigging and animation
- Unknown Worlds: Original Ice Dragon concept art

Special thanks to Cr00L, Hilmi and Shoob 

![Ice dragon side view](https://media.githubusercontent.com/media/ThePlagueSpreads/IceDragonMod/main/IceDragonAssets/Assets/Promotional/IceDragonRender_Side.png)
