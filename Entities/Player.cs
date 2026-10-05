using System.Numerics;
using Raylib_cs;

namespace My2DGame.Entities;

/// <summary>
/// Simple circle player with smooth acceleration, sprint and world-bounds
/// clamping. When you add real art, replace the shape drawing in
/// <see cref="Draw"/> with sprite draws — the movement logic stays.
/// </summary>
public sealed class Player
{
    private const float WalkSpeed = 250f;
    private const float SprintMultiplier = 1.7f;
    private const float Acceleration = 10f;

    private static readonly Color BodyColor = new(72, 178, 255, 255);

    private Vector2 _velocity;

    public Player(Vector2 startPosition) => Position = startPosition;

    public Vector2 Position { get; private set; }
    public float Radius { get; } = 16f;
    public Vector2 Facing { get; private set; } = Vector2.UnitX;
    public float Speed { get; set; } = WalkSpeed;

    public void Update(float dt, Rectangle worldBounds)
    {
        Vector2 input = ReadInput();
        if (input.LengthSquared() > 0f) input = Vector2.Normalize(input);

        float speed = Speed * (Raylib.IsKeyDown(KeyboardKey.LeftShift) ? SprintMultiplier : 1f);
        Vector2 targetVelocity = input * speed;

        // Frame-rate independent smoothing toward the target velocity.
        _velocity = Vector2.Lerp(_velocity, targetVelocity, 1f - MathF.Exp(-Acceleration * dt));
        Position += _velocity * dt;

        if (input.LengthSquared() > 0f) Facing = Vector2.Normalize(input);

        Position = new Vector2(
            Math.Clamp(Position.X, worldBounds.X + Radius, worldBounds.X + worldBounds.Width - Radius),
            Math.Clamp(Position.Y, worldBounds.Y + Radius, worldBounds.Y + worldBounds.Height - Radius));
    }

    public void Draw(float time)
    {
        _ = time; // reserved for animating the body later

        // Soft glow, body, and a small direction marker.
        Raylib.DrawCircleV(Position, Radius * 1.9f, Raylib.ColorAlpha(BodyColor, 0.22f));
        Raylib.DrawCircleV(Position, Radius, BodyColor);
        Raylib.DrawLineEx(Position + Facing * 4f, Position + Facing * (Radius + 9f), 4f, Color.White);
    }

    private static Vector2 ReadInput()
    {
        Vector2 input = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up)) input.Y -= 1f;
        if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down)) input.Y += 1f;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) input.X -= 1f;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) input.X += 1f;
        return input;
    }
}