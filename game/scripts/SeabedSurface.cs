#nullable enable
using Godot;

namespace Fish;

/// <summary>One continuous authored basin surface; visuals and two-sided collision share its mesh.</summary>
public partial class SeabedSurface : StaticBody3D
{
    public const float HalfExtent = 42.0f;
    public const float CellSize = 1.5f;
    public const int CellsPerSide = 56;
    // Center of the reachable authored deep shelf, away from the outer rise.
    public const float DeepShelfReferenceZ = -21.0f;
    private ShaderMaterial? _waterMaterial;

    public void SetWaterColumn(float surfaceY, float deepY)
    {
        if (!float.IsFinite(surfaceY) || !float.IsFinite(deepY) || deepY >= surfaceY)
            throw new System.ArgumentOutOfRangeException(nameof(deepY), "Invalid water-column anchors");
        _waterMaterial?.SetShaderParameter("water_surface_y", surfaceY);
        _waterMaterial?.SetShaderParameter("water_deep_y", deepY);
    }

    public bool Build(StandardMaterial3D[] materials)
    {
        if (materials.Length != HabitatPalette.Names.Length) return false;
        var shader = ResourceLoader.Load<Shader>("res://shaders/seabed_blend.gdshader");
        if (shader is null) return false;
        var material = new ShaderMaterial { Shader = shader, ResourceName = "P2 Blended Seabed" };
        _waterMaterial = material;
        for (var index = 0; index < materials.Length; index++)
        {
            var prefix = new[] { "coast", "shell", "gravel", "rock" }[index];
            if (materials[index].AlbedoTexture is null || materials[index].NormalTexture is null ||
                materials[index].RoughnessTexture is null) return false;
            material.SetShaderParameter(prefix + "_color", materials[index].AlbedoTexture);
            material.SetShaderParameter(prefix + "_normal", materials[index].NormalTexture);
            material.SetShaderParameter(prefix + "_roughness", materials[index].RoughnessTexture);
        }
        using var builder = new SurfaceTool();
        builder.Begin(Mesh.PrimitiveType.Triangles);
        builder.SetMaterial(material);

        for (var z = 0; z < CellsPerSide; z++)
        for (var x = 0; x < CellsPerSide; x++)
        {
            var left = -HalfExtent + x * CellSize;
            var near = -HalfExtent + z * CellSize;
            var right = left + CellSize;
            var far = near + CellSize;
            var a = Point(left, near);
            var b = Point(left, far);
            var c = Point(right, near);
            var d = Point(right, far);
            // Winding faces upward; both triangles share the same analytic corner heights.
            AddTriangle(builder, a, c, b);
            AddTriangle(builder, c, d, b);
        }

        var mesh = new ArrayMesh();
        builder.Commit(mesh);
        var shape = mesh.CreateTrimeshShape() as ConcavePolygonShape3D;
        if (shape is null) return false;
        shape.BackfaceCollision = true;
        GetNode<MeshInstance3D>("Mesh").Mesh = mesh;
        GetNode<CollisionShape3D>("Collision").Shape = shape;
        return true;
    }

    public float GetHeightAt(float x, float z)
    {
        // Oblique, unequal transitions overlap in perspective instead of forming
        // four long parallel steps. They remain slopes on one collidable surface.
        var deepToLower = z - 0.05f * x - 3.1f * Mathf.Sin(0.16f * x + 0.3f);
        var lowerToMiddle = z - 0.20f * x + 2.8f * Mathf.Sin(0.12f * x - 0.7f);
        var middleToUpper = z + 0.08f * x - 2.5f * Mathf.Sin(0.17f * x + 1.4f);
        var upperToShallow = z - 0.12f * x + 3.0f * Mathf.Sin(0.11f * x + 0.6f);
        var shelves = -9.4f
            + 3.3f * Smooth(-21.5f, -14.0f, deepToLower)
            + 3.2f * Smooth(-10.5f, -3.0f, lowerToMiddle)
            + 3.1f * Smooth(2.0f, 9.5f, middleToUpper)
            + 3.2f * Smooth(12.0f, 19.0f, upperToShallow);
        var folds = 0.52f * Mathf.Sin(0.17f * x + 0.11f * z) * Mathf.Sin(0.20f * z)
            + 0.30f * Mathf.Sin(0.34f * x - 0.13f * z);
        var westernShoulder = 1.5f * Ellipse(x, z, -22.0f, -2.0f, 11.0f, 15.0f);
        var easternChannel = -1.5f * Ellipse(x, z, 23.0f, 7.0f, 8.0f, 20.0f);
        var cut = -1.0f * Ellipse(x, z, -12.0f, 18.0f, 7.0f, 12.0f);
        var ridge = 2.1f * Ellipse(x, z, 29.0f, -8.0f, 8.0f, 13.0f)
            + 1.8f * Ellipse(x, z, -30.0f, 19.0f, 7.0f, 10.0f);
        // The collision-only limits contain play; a low broken outer rise avoids
        // advertising one continuous basin wall across the horizon.
        var edge = Mathf.Max(Mathf.Abs(x + 2.3f * Mathf.Sin(0.10f * z)),
            Mathf.Abs(z + 2.0f * Mathf.Sin(0.12f * x)));
        var outerRise = (4.0f + 2.0f * Mathf.Sin(0.12f * x + 0.09f * z)
            * Mathf.Sin(0.08f * z - 0.05f * x)) * Smooth(31.0f, 40.0f, edge);
        return shelves + folds + westernShoulder + easternChannel + cut + ridge + outerRise;
    }

    private static float Ellipse(float x, float z, float cx, float cz, float rx, float rz)
    {
        var dx = (x - cx) / rx;
        var dz = (z - cz) / rz;
        return 1.0f - Smooth(0.0f, 1.0f, dx * dx + dz * dz);
    }

    private Vector3 Point(float x, float z) => new(x, GetHeightAt(x, z), z);

    private void AddTriangle(SurfaceTool builder, Vector3 a, Vector3 b, Vector3 c)
    {
        AddVertex(builder, a);
        AddVertex(builder, b);
        AddVertex(builder, c);
    }

    private void AddVertex(SurfaceTool builder, Vector3 point)
    {
        const float sample = 0.15f;
        var xSlope = (GetHeightAt(point.X + sample, point.Z) -
            GetHeightAt(point.X - sample, point.Z)) / (2 * sample);
        var zSlope = (GetHeightAt(point.X, point.Z + sample) -
            GetHeightAt(point.X, point.Z - sample)) / (2 * sample);
        builder.SetNormal(new Vector3(-xSlope, 1, -zSlope).Normalized());
        builder.SetUV(new Vector2(point.X, point.Z) * 0.24f);
        builder.AddVertex(point);
    }

    private static float Smooth(float start, float end, float value)
    {
        var t = Mathf.Clamp((value - start) / (end - start), 0, 1);
        return t * t * (3 - 2 * t);
    }
}
