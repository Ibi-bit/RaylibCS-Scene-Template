using System.Text.Json;

namespace My2DGame.Core;

/// <summary>
/// Serializable window/settings config. Saved as <c>gameconfig.json</c> next to the
/// executable on first run, then loaded from there — tweaking a string never
/// requires touching code.
/// </summary>
public sealed class GameConfig
{
    private const string FileName = "gameconfig.json";
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, FileName);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string WindowTitle { get; set; } = "My 2D Game - raylib + ImGui";
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 720;
    public int TargetFps { get; set; } = 60;
    public bool EnableVsync { get; set; }
    public bool ResizableWindow { get; set; } = true;
    public bool StartMaximized { get; set; }
    public bool ShowDebugWindow { get; set; } = true;

    /// <summary>Scene to boot into: "Title" or "Game". Handy when iterating on gameplay.</summary>
    public string BootScene { get; set; } = "Title";

    public static GameConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                GameConfig? config = JsonSerializer.Deserialize<GameConfig>(File.ReadAllText(ConfigPath), JsonOptions);
                if (config is not null) return config;
            }
        }
        catch (JsonException)
        {
            // Broken config? Fall back to defaults — the next Save() rewrites it.
        }

        GameConfig fresh = new();
        fresh.Save();
        return fresh;
    }

    public void Save() => File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, JsonOptions));
}