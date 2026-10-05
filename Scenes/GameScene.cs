using System.Numerics;
using My2DGame.Audio;
using My2DGame.Core;
using My2DGame.Entities;
using Raylib_cs;

namespace My2DGame.Scenes;

/// <summary>
/// The playable demo scene: a tiled world with bounds, a player controlled by
/// WASD, a smooth-following zoomable camera, collectible coins and a HUD.
/// It doubles as a reference for the patterns you'll reuse in your own scene:
///   - world rendering inside a Camera2D,
///   - view-clipped tile/grid drawing,
///   - entities updated in Update() and drawn in Draw(), with enabled flags
///     (ShowGrid / ShowHud / CameraFollow) that the ImGui debug window drives.
/// </summary>
public sealed class GameScene : Scene
{
    public override string Name => "Game";

    public static readonly Rectangle WorldBounds = new(0, 0, 3200, 2400);

    private const float TileSize = 64f;
    private const float MinZoom = 0.25f;
    private const float MaxZoom = 4f;
    private const int TotalCoins = 8;

    private readonly Player _player;
    private readonly List<Collectible> _collectibles = new();
    private readonly CoinBlip _blip = new();

    private Camera2D _camera;
    private Texture2D _tileTexture;

    private float _targetZoom = 1f;
    private int _score;
    private string _message = "";
    private float _messageTimer;

    public GameScene()
    {
        _player = new Player(new Vector2(WorldBounds.Width / 2f, WorldBounds.Height / 2f));
    }

    public bool ShowGrid { get; set; } = true;
    public bool ShowHud { get; set; } = true;
    public bool CameraFollow { get; set; } = true;

    public Camera2D Camera => _camera;
    public Player Player => _player;
    public int Score => _score;
    public int CoinCount => _collectibles.Count;

    public override void OnEnter()
    {
        _tileTexture = CreateTileTexture();
        SpawnCoins();
        _score = 0;
        _targetZoom = 1f;

        _camera = new Camera2D(
            new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f),
            _player.Position,
            0f,
            1f);

        ShowMessage("Collect the coins!");
    }

    public override void OnExit() => Raylib.UnloadTexture(_tileTexture);

    public override void Update(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) { Scenes.ChangeTo<TitleScene>(); return; }
        if (Raylib.IsKeyPressed(KeyboardKey.R)) { Scenes.RestartCurrent(); return; }
        if (Raylib.IsKeyPressed(KeyboardKey.G)) ShowGrid = !ShowGrid;

        _player.Update(dt, WorldBounds);
        UpdateCamera(dt);

        int collectedThisFrame = 0;
        foreach (Collectible coin in _collectibles)
        {
            coin.Update(dt);
            if (coin.TryCollect(_player))
            {
                _score++;
                collectedThisFrame++;
            }
        }

        if (collectedThisFrame > 0)
        {
            _blip.Play();
            if (_score >= CoinCount) ShowMessage("All coins collected! Press R to play again.");
        }

        if (_messageTimer > 0f) _messageTimer -= dt;
    }

    public override void Draw()
    {
        Raylib.BeginMode2D(_camera);

        DrawGround();
        DrawWorldBorder();

        foreach (Collectible coin in _collectibles)
            coin.Draw((float)Raylib.GetTime());

        _player.Draw((float)Raylib.GetTime());

        Raylib.EndMode2D();

        if (ShowHud) DrawHud();
    }

    private void UpdateCamera(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E)) _targetZoom = 1f;

        float wheel = Raylib.GetMouseWheelMove();
        if (MathF.Abs(wheel) > 0f)
            _targetZoom = Math.Clamp(_targetZoom * MathF.Pow(1.15f, wheel), MinZoom, MaxZoom);

        // Smooth, frame-rate independent zoom and follow.
        _camera.Zoom = _camera.Zoom + (_targetZoom - _camera.Zoom) * (1f - MathF.Exp(-10f * dt));

        Vector2 focus = CameraFollow
            ? _player.Position
            : new Vector2(WorldBounds.Width / 2f, WorldBounds.Height / 2f);
        _camera.Target = Vector2.Lerp(_camera.Target, focus, 1f - MathF.Exp(-6f * dt));

        _camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
    }

    /// <summary>Checkerboard tiles + optional grid lines, clipped to the camera view.</summary>
    private void DrawGround()
    {
        Vector2 viewTopLeft = Raylib.GetScreenToWorld2D(Vector2.Zero, _camera);
        Vector2 viewBottomRight = Raylib.GetScreenToWorld2D(
            new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), _camera);

        float left = MathF.Max(viewTopLeft.X, WorldBounds.X);
        float top = MathF.Max(viewTopLeft.Y, WorldBounds.Y);
        float right = MathF.Min(viewBottomRight.X, WorldBounds.X + WorldBounds.Width);
        float bottom = MathF.Min(viewBottomRight.Y, WorldBounds.Y + WorldBounds.Height);

        if (right < left || bottom < top) return;

        int tileX0 = (int)MathF.Floor(left / TileSize);
        int tileY0 = (int)MathF.Floor(top / TileSize);
        int tileX1 = (int)MathF.Ceiling(right / TileSize);
        int tileY1 = (int)MathF.Ceiling(bottom / TileSize);

        for (int ty = tileY0; ty <= tileY1; ty++)
        {
            for (int tx = tileX0; tx <= tileX1; tx++)
            {
                Raylib.DrawTextureEx(
                    _tileTexture,
                    new Vector2(tx * TileSize, ty * TileSize),
                    0f,
                    1f,
                    Color.White);
            }
        }

        if (!ShowGrid) return;

        Color gridColor = Raylib.ColorAlpha(Color.White, 0.06f);
        int gridX0 = (int)MathF.Ceiling(left / TileSize);
        int gridY0 = (int)MathF.Ceiling(top / TileSize);

        for (float x = gridX0 * TileSize; x <= right; x += TileSize)
            Raylib.DrawLineEx(new Vector2(x, top), new Vector2(x, bottom), 1f, gridColor);
        for (float y = gridY0 * TileSize; y <= bottom; y += TileSize)
            Raylib.DrawLineEx(new Vector2(left, y), new Vector2(right, y), 1f, gridColor);
    }

    private static void DrawWorldBorder()
    {
        const float thickness = 6f;
        Color color = new(140, 62, 62, 255);
        var origin = new Vector2(WorldBounds.X, WorldBounds.Y);
        var bounds = new Vector2(WorldBounds.Width, WorldBounds.Height);

        Raylib.DrawRectangleV(origin, new Vector2(bounds.X, thickness), color);
        Raylib.DrawRectangleV(origin + new Vector2(0f, bounds.Y - thickness), new Vector2(bounds.X, thickness), color);
        Raylib.DrawRectangleV(origin, new Vector2(thickness, bounds.Y), color);
        Raylib.DrawRectangleV(origin + new Vector2(bounds.X - thickness, 0f), new Vector2(thickness, bounds.Y), color);
    }

    private void DrawHud()
    {
        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();

        string coins = $"COINS  {_score} / {CoinCount}";
        int coinWidth = Raylib.MeasureText(coins, 24);
        Raylib.DrawText(coins, width - coinWidth - 20, 16, 24, Color.Gold);

        const string hint = "[WASD] move   [Shift] sprint   [Wheel] zoom   [E] reset zoom   [G] grid   [R] restart   [Esc] menu";
        int hintWidth = Raylib.MeasureText(hint, 16);
        Raylib.DrawText(hint, (width - hintWidth) / 2, height - 30, 16, Raylib.ColorAlpha(Color.White, 0.45f));

        if (_messageTimer > 0f)
        {
            float alpha = Math.Clamp(_messageTimer / 0.4f, 0f, 1f) * 0.95f;
            int messageWidth = Raylib.MeasureText(_message, 28);
            Raylib.DrawText(_message, (width - messageWidth) / 2, height / 2 - 40, 28, Raylib.ColorAlpha(Color.White, alpha));
        }
    }

    private void SpawnCoins()
    {
        _collectibles.Clear();
        const float margin = 120f;

        for (int i = 0; i < TotalCoins; i++)
        {
            var position = new Vector2(
                margin + Random.Shared.NextSingle() * (WorldBounds.Width - margin * 2f),
                margin + Random.Shared.NextSingle() * (WorldBounds.Height - margin * 2f));
            _collectibles.Add(new Collectible(position));
        }
    }

    private static Texture2D CreateTileTexture()
    {
        Image image = Raylib.GenImageChecked(
            (int)TileSize, (int)TileSize, 2, 2,
            new Color(32, 34, 44, 255),
            new Color(26, 28, 36, 255));
        Texture2D texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
        return texture;
    }

    private void ShowMessage(string message)
    {
        _message = message;
        _messageTimer = 2.5f;
    }
}