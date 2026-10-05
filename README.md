# My 2D Game — raylib + Dear ImGui template (C#)

A clean, ready-to-extend template for 2D games in C# using:

| Library   | Role                     | NuGet package | Version |
|-----------|--------------------------|---------------|---------|
| raylib    | Window, input, rendering, audio | `Raylib-cs`  | 7.0.1 (pinned) |
| Dear ImGui| Immediate-mode debug UI / editor tools | `rlImgui-cs` + `ImGui.NET` | 3.2.0 / 1.91.6.1 |

> **Version pinning:** `rlImgui-cs 3.2.0` was built against `Raylib-cs 7.0.1`.
> Keep the pair in lockstep — a newer raylib may change the native ABI that the
> ImGui backend depends on.

## Run it

```bash
cd My2DGame
dotnet run
```

`gameconfig.json` (window size, vsync, target FPS, boot scene, ...) is generated
next to the executable on first run — edit it there, no recompile needed.
`BootScene` can be `"Title"` or `"Game"` to skip the menu while iterating.

For CI/screenshots there's a test hook: set the `GAME_SHOT_PATH` env var (and
optionally `GAME_SHOT_FRAME`, default 120) and the game saves a screenshot to
your current working directory after that many frames, then keeps running.

## Controls

| Input | Action |
|-------|--------|
| `WASD` / arrows | Move |
| `Shift` | Sprint |
| Mouse wheel | Zoom camera |
| `E` | Reset zoom |
| `G` | Toggle grid |
| `R` | Restart the game scene |
| `Esc` | Title: quit · Game: back to title |
| `F1` | ImGui demo window |
| `F2` | Toggle debug window |
| `F10` | Save a screenshot (`screenshot_TIMESTAMP.png` next to the executable) |

The debug window (top-left, draggable) shows FPS, frame time, mouse position in
screen and world space, player/coin state, and scene actions. It is the
template's "editor": add sliders, toggles and tools there.

## Project layout

```
My2DGame/
├── Program.cs            Entry point: load config, create Game, run
├── Core/
│   ├── Game.cs           Window + ImGui setup, main loop (update → draw → UI)
│   ├── GameConfig.cs     JSON settings file (auto-created)
│   ├── Scene.cs          Base class: OnEnter / Update / Draw / DrawDebugUI / OnExit
│   └── SceneManager.cs   Lazy scene creation, switching, restarting
├── Scenes/
│   ├── TitleScene.cs     Menu with animated starfield
│   └── GameScene.cs      Playable demo: tiled world, camera, coins, HUD
├── Entities/
│   ├── Player.cs         Movement, sprint, world bounds
│   └── Collectible.cs    Coin: pick-up, respawn, bob + spin animation
├── Audio/
│   └── CoinBlip.cs       Procedural coin sound (no asset files needed)
├── UI/
│   └── DebugUI.cs        ImGui overlay: hotkeys, debug window, demo toggle
└── Content/              Put textures / fonts / audio here (copied to output)
```

## Making it your own

1. **Add a scene** — subclass `Scene`, register it in `Game.cs`:
   ```csharp
   _scenes.Register<PauseScene>(() => new PauseScene());
   ```
   and switch with `Scenes.ChangeTo<PauseScene>()`.
2. **Add assets** — drop files into `Content/` and load them relative to
   `Raylib.GetApplicationDirectory()` (or `AppContext.BaseDirectory`).
3. **Game feel** — the camera follow/zoom and player acceleration use
   frame-rate independent `exp`-based smoothing; reuse the same pattern in your
   own systems.
4. **Editor tools** — any scene can draw ImGui widgets in `DrawDebugUI`, and
   the global overlay lives in `UI/DebugUI.cs`.

## Troubleshooting

- **Missing window/blank screen** — make sure your machine has a display and,
  on Linux, the raylib dependencies (`libasound2`/`alsa-lib`, `libx11`, etc.).
- **Audio device** — raylib falls back gracefully, but if `InitAudioDevice`
  fails the game still runs (sound just won't play).