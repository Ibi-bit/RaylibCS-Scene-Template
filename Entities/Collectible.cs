using System.Numerics;
using Raylib_cs;

namespace My2DGame.Entities;

/// <summary>
/// A collectible coin: bobs and "spins" (fake 3D via ellipse squash), gets
/// collected on overlap with the player and respawns a few seconds later.
/// </summary>
public sealed class Collectible
{
    private const float RespawnDelay = 6f;
    private static readonly Color Gold = new(255, 204, 51, 255);
    private static readonly Color DarkGold = new(180, 130, 20, 255);

    private readonly float _phase = Random.Shared.NextSingle() * MathF.Tau;
    private float _respawnTimer;

    public Collectible(Vector2 position) => Position = position;

    public Vector2 Position { get; }
    public float Radius { get; } = 14f;
    public bool IsCollected { get; private set; }

    public void Update(float dt)
    {
        if (!IsCollected) return;
        _respawnTimer -= dt;
        if (_respawnTimer <= 0f) IsCollected = false;
    }

    public bool TryCollect(Player player)
    {
        if (IsCollected) return false;

        float minDistance = Radius + player.Radius;
        if (Vector2.DistanceSquared(player.Position, Position) > minDistance * minDistance) return false;

        IsCollected = true;
        _respawnTimer = RespawnDelay;
        return true;
    }

    public void Draw(float time)
    {
        if (IsCollected) return;

        float bob = MathF.Sin(time * 2.2f + _phase) * 3f;
        float spin = MathF.Sin(time * 2.8f + _phase * 1.7f);
        Vector2 center = Position + new Vector2(0f, bob);

        // Ellipse squash simulates the coin rotating on its vertical axis.
        float verticalRadius = MathF.Max(2f, Radius * (0.35f + MathF.Abs(spin) * 0.65f));

        Raylib.DrawCircleV(center, Radius * 1.5f, Raylib.ColorAlpha(Gold, 0.18f));
        Raylib.DrawEllipse((int)center.X, (int)center.Y, Radius, verticalRadius, Gold);
        Raylib.DrawEllipse((int)center.X, (int)center.Y, Radius * 0.55f, verticalRadius * 0.55f, DarkGold);
    }
}