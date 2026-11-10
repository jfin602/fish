using Godot;

namespace Fish;

/// <summary>Checked local P2 seabed sets. P4 can reuse these materials for placed terrain pieces.</summary>
public static class HabitatPalette
{
    public const int CoastSand = 0;
    public const int ShellSand = 1;
    public const int Gravel = 2;
    public const int ExposedRock = 3;
    public static readonly string[] Names = ["Coast Sand 01", "Ground060", "Gravel041", "Rock063"];

    private const string P1 = "res://assets/vendor-local/coast_sand_01/";
    private const string P2 = "res://assets/vendor-local/p2-environment/ambientcg/";

    public static bool TryLoad(out StandardMaterial3D[] materials, out string diagnostic)
    {
        materials = [];
        if (!TryTexture("Coast Sand 01", P1 + "coast_sand_01_Diffuse_2k.jpg", out var coastColor, out diagnostic) ||
            !TryTexture("Coast Sand 01", P1 + "coast_sand_01_nor_gl_2k.jpg", out var coastNormal, out diagnostic) ||
            !TryTexture("Coast Sand 01", P1 + "coast_sand_01_Rough_2k.jpg", out var coastRoughness, out diagnostic))
            return false;

        var result = new StandardMaterial3D[Names.Length];
        result[CoastSand] = Make("Coast Sand 01", coastColor, coastNormal, coastRoughness);
        for (var index = 1; index < Names.Length; index++)
        {
            var name = Names[index];
            var stem = P2 + name + "/" + name + "_2K-JPG_";
            if (!TryTexture(name, stem + "Color.jpg", out var color, out diagnostic) ||
                !TryTexture(name, stem + "NormalGL.jpg", out var normal, out diagnostic) ||
                !TryTexture(name, stem + "Roughness.jpg", out var roughness, out diagnostic) ||
                !TryTexture(name, stem + "AmbientOcclusion.jpg", out _, out diagnostic) ||
                !TryTexture(name, stem + "Displacement.jpg", out _, out diagnostic))
                return false;
            result[index] = Make(name, color, normal, roughness);
        }
        materials = result;
        diagnostic = "";
        return true;
    }

    private static StandardMaterial3D Make(string name, Texture2D color, Texture2D normal, Texture2D roughness) => new()
    {
        ResourceName = name,
        AlbedoTexture = color,
        NormalEnabled = true,
        NormalTexture = normal,
        RoughnessTexture = roughness,
        Roughness = 1.0f,
    };

    private static bool TryTexture(string item, string path, out Texture2D texture, out string diagnostic)
    {
        texture = null!;
        if (!FileAccess.FileExists(path))
        {
            diagnostic = $"FISH_ASSET_MISSING item={item} path={path} action=prepare-source-under-game/assets/vendor-local";
            return false;
        }
        texture = ResourceLoader.Load<Texture2D>(path)!;
        if (texture is null || texture.GetWidth() < 1024 || texture.GetHeight() < 1024)
        {
            diagnostic = $"FISH_ASSET_INVALID item={item} path={path} action=run-tools-dev-import-and-check-maps";
            return false;
        }
        diagnostic = "";
        return true;
    }
}
