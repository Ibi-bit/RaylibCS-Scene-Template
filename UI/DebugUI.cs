using System.Numerics;
using ImGuiNET;
using My2DGame.Core;
using My2DGame.Scenes;
using Raylib_cs;

namespace My2DGame.UI;

/// <summary>
/// The always-available ImGui overlay: global hotkeys, a scene-aware debug
/// window (FPS, timings, mouse/world position, scene-specific controls) and
/// the ImGui demo window. This is the natural place to add editor tools,
/// a console, or draw-call stats later.
/// </summary>
public sealed class DebugUI
{
    private readonly SceneManager _scenes;

    private bool _showDebugWindow;
    private bool _showDemoWindow;

    public DebugUI(SceneManager scenes, bool showDebugWindowOnStart)
    {
        _scenes = scenes;
        _showDebugWindow = showDebugWindowOnStart;
    }

    /// <summary>Global hotkeys, checked before a scene's Update.</summary>
    public void ProcessShortcuts()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F1)) _showDemoWindow = !_showDemoWindow;
        if (Raylib.IsKeyPressed(KeyboardKey.F2)) _showDebugWindow = !_showDebugWindow;
        if (Raylib.IsKeyPressed(KeyboardKey.F10))
            Raylib.TakeScreenshot(Path.Combine(AppContext.BaseDirectory, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"));
    }

    public void Draw(Scene? scene)
    {
        if (_showDemoWindow) ImGui.ShowDemoWindow(ref _showDemoWindow);
        if (!_showDebugWindow) return;

        ImGui.SetNextWindowPos(new Vector2(12f, 12f), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Debug"))
        {
            ImGui.Text($"Scene: {scene?.Name ?? "(none)"}");
            ImGui.Separator();

            ImGui.Text($"FPS: {Raylib.GetFPS()}   Frame: {Raylib.GetFrameTime() * 1000f:F2} ms");
            ImGui.Text($"Window: {Raylib.GetScreenWidth()} x {Raylib.GetScreenHeight()}   Time: {Raylib.GetTime():F1}s");
            ImGui.Text($"Mouse (screen): {Raylib.GetMousePosition()}");

            switch (scene)
            {
                case GameScene game:
                    DrawGameScenePanel(game);
                    break;

                case TitleScene:
                    ImGui.Separator();
                    if (ImGui.Button("Start Game")) _scenes.ChangeTo<GameScene>();
                    ImGui.SameLine();
                    if (ImGui.Button("Quit")) Raylib.CloseWindow();
                    break;
            }

            ImGui.Separator();

            float uiScale = ImGui.GetIO().FontGlobalScale;
            if (ImGui.SliderFloat("UI scale", ref uiScale, 0.75f, 2f))
                ImGui.GetIO().FontGlobalScale = uiScale;

            ImGui.TextColored(new Vector4(0.55f, 0.55f, 0.6f, 1f), "F1 demo   F2 toggle this window");
        }
        ImGui.End();
    }

    private void DrawGameScenePanel(GameScene game)
    {
        Vector2 worldMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), game.Camera);
        ImGui.Text($"Mouse (world): ({worldMouse.X:F0}, {worldMouse.Y:F0})   Zoom: {game.Camera.Zoom:F2}");
        ImGui.Text($"Player: ({game.Player.Position.X:F0}, {game.Player.Position.Y:F0})");
        ImGui.Text($"Coins: {game.Score} / {game.CoinCount}");
        ImGui.Separator();

        bool showGrid = game.ShowGrid;
        if (ImGui.Checkbox("Show grid", ref showGrid)) game.ShowGrid = showGrid;

        bool follow = game.CameraFollow;
        if (ImGui.Checkbox("Follow player", ref follow)) game.CameraFollow = follow;

        bool showHud = game.ShowHud;
        if (ImGui.Checkbox("Show HUD", ref showHud)) game.ShowHud = showHud;

        ImGui.Separator();

        if (ImGui.Button("Restart scene (R)")) _scenes.RestartCurrent();
        ImGui.SameLine();
        if (ImGui.Button("Back to title")) _scenes.ChangeTo<TitleScene>();
    }
}