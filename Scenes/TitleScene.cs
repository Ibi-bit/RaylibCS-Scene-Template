using System.Numerics;
using My2DGame.Core;
using Raylib_cs;

namespace My2DGame.Scenes;

/// <summary>
/// Main menu: a drifting starfield, the game title, and a blinking start
/// prompt. Enter / Space / click starts the game, Esc quits.
/// </summary>
public sealed class TitleScene : Scene
{
    private const int StarCount = 160;

    public override string Name => "Title";

    private readonly struct Star
    {
        public Star(Vector2 position, float speed, float size, float brightness)
        {
            Position = position;
            Speed = speed;
            Size = size;
            Brightness = brightness;
        }

        public Vector2 Position { get; }
        public float Speed { get; }
        public float Size { get; }
        public float Brightness { get; }
    }

    private readonly Star[] _stars = new Star[StarCount];

    public TitleScene()
    {
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i] = new Star(
                new Vector2(Random.Shared.NextSingle() * 1920f, Random.Shared.NextSingle() * 1080f),
                30f + Random.Shared.NextSingle() * 90f,
                1f + Random.Shared.NextSingle() * 2.2f,
                0.25f + Random.Shared.NextSingle() * 0.75f);
        }
    }

    public override void Update(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) ||
            Raylib.IsKeyPressed(KeyboardKey.Space) ||
            Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Scenes.ChangeTo<GameScene>();
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) Raylib.CloseWindow();

        float width = Raylib.GetScreenWidth();
        float height = Raylib.GetScreenHeight();
        float time = (float)Raylib.GetTime();

        for (int i = 0; i < _stars.Length; i++)
        {
            Star star = _stars[i];

            float x = star.Position.X + MathF.Sin(time * 0.6f + i) * 6f * dt;
            float y = star.Position.Y + star.Speed * dt;
            if (x > width) x -= width;
            if (y > height) y -= height;

            _stars[i] = new Star(new Vector2(x, y), star.Speed, star.Size, star.Brightness);
        }
    }

    public override void Draw()
    {
        float width = Raylib.GetScreenWidth();
        float height = Raylib.GetScreenHeight();

        foreach (Star star in _stars)
            Raylib.DrawCircleV(star.Position, star.Size, Raylib.ColorAlpha(Color.White, star.Brightness * 0.8f));

        const string title = "MY 2D GAME";
        const string subtitle = "raylib + Dear ImGui   |   C# 2D game template";
        const string prompt = "Press ENTER / SPACE / Click to start  (Esc quits)";

        int titleSize = 64;
        int titleX = (int)((width - Raylib.MeasureText(title, titleSize)) / 2f);
        int titleY = (int)(height / 3f);

        Raylib.DrawText(title, titleX + 3, titleY + 3, titleSize, Raylib.ColorAlpha(Color.Black, 0.8f));
        Raylib.DrawText(title, titleX, titleY, titleSize, Color.Gold);

        int subX = (int)((width - Raylib.MeasureText(subtitle, 20)) / 2f);
        Raylib.DrawText(subtitle, subX, titleY + titleSize + 12, 20, Color.LightGray);

        float blink = 0.5f + 0.5f * MathF.Sin((float)Raylib.GetTime() * 3.5f);
        int promptX = (int)((width - Raylib.MeasureText(prompt, 24)) / 2f);
        Raylib.DrawText(prompt, promptX, titleY + titleSize + 58, 24, Raylib.ColorAlpha(Color.White, blink));
    }
}