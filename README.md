# Right Click Exit

A small quality-of-life mod for **Schedule I** (TVGS). Right-click now closes
menus, dialogue and other screens, the same way Escape does.

Without the mod, right-click only works as "back" in some screens. Talking to
NPCs, leaving the ATM, shops, the pawn shop, deal handovers, casino tables and
many others still need Escape. With the mod, right-click handles all of them.

## Features

- **Right-click closes any screen Escape closes.** That includes dialogue, ATM,
  shops, pawn shop, deal handovers, casino tables, vehicle mods, character
  customization, the hospital bill, graffiti, the pause menu, main-menu
  sub-screens and more. Screens that already close on right-click are unchanged.
- **Each screen closes through its own code.** The mod replays the game's own
  Escape handling instead of hiding UI, so sounds, animations, confirmation
  prompts and multiplayer sync behave exactly as they do with Escape. Dialogue
  that the game won't let you leave still can't be left.
- **Never opens the pause menu.** Right-click during normal gameplay does exactly
  what it did before: aiming, vehicles and skateboards are untouched.
- **Respects right-click where it already means something.** Inventory slots,
  station tasks (multi-grab) and the management clipboard selectors (clear
  selection) keep their normal right-click behavior.
- **No game methods are patched.** The mod adds one entry to the game's own exit
  listener list. Nothing runs per frame except a once-per-second check.
- Configurable, with builds for both the IL2CPP (main) and Mono (alternate) game
  branches.

## Requirements

- Schedule I (tested on v0.4.7f12)
- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.7.0 or newer
  (tested on v0.7.3)

## Installation

1. Install MelonLoader into Schedule I and launch the game once.
2. Download the build that matches your game branch:
   - **IL2CPP**: the normal Steam version. Most players want this one.
   - **Mono**: only if you opted into the `alternate` beta branch in Steam.
3. Extract the zip into the game folder, so `RightClickExit.dll` ends up in
   `Schedule I\Mods\`.
4. Launch the game. The MelonLoader console should show
   `[Right Click Exit] v1.0.0 loaded for IL2CPP (main branch).`

To uninstall, delete `Mods\RightClickExit.dll`.

## Configuration

Settings are saved to `Schedule I\UserData\MelonPreferences.cfg` after the first
launch. Edit them with the game closed:

```toml
[RightClickExit]
Enabled = true          # Master switch
CloseTasks = false      # Also cancel station tasks (mixing, packaging, chemistry...)
VerboseLogging = false  # Log every right-click the mod handles (for bug reports)
```

`CloseTasks` is off by default because several station tasks use right-click to
grab multiple items at once.

## Compatibility

- Works in single-player and multiplayer. It runs entirely on your own client.
- Other mods' screens that close on Escape through the game's exit system also
  close on right-click.
- If a game update changes the menu code, the mod logs a clear error and stops
  handling clicks. It won't crash the game.

## How it works

See [docs/HOW_IT_WORKS.md](docs/HOW_IT_WORKS.md).

## Building from source

You need the .NET 8 SDK (or newer) and your own copy of the game. Game files are
never included in this repository.

```sh
# IL2CPP (main branch). Run the game with MelonLoader once first.
dotnet build src -c Il2Cpp

# Mono (alternate branch)
dotnet build src -c Mono

# Release zips in dist/
dotnet build src -c Il2Cpp -t:Package
dotnet build src -c Mono -t:Package
```

By default the build looks for the game in
`C:\Program Files (x86)\Steam\steamapps\common\Schedule I`. To use another path,
copy `LocalPaths.props.example` to `LocalPaths.props` and edit it, or pass
`-p:GameDir="..."` on the command line.

The IL2CPP branch has no `Schedule I_Data\Managed` folder. To build the Mono
version without switching branches, keep a copy of that folder from the
alternate branch and set `ManagedDir` in `LocalPaths.props`.

The version number is set once, in `src/RightClickExit.csproj` (`<Version>`).

## License

[MIT](LICENSE). This is a fan-made mod. It is not affiliated with or endorsed by
TVGS.
