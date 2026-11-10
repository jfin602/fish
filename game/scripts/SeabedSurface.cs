using Godot;

namespace Fish;

/// <summary>One continuous authored basin surface; visuals and two-sided collision share its mesh.</summary>
public partial class SeabedSurface : StaticBody3D
{
    public const float HalfExtent = 42.0f;
    public const float CellSize = 1.5f;
    public const int CellsPerSide = 56;

    public bool Build(StandardMaterial3D[] materials)
    {
        if (materials.Length != HabitatPalette.Names.Length) return false;
        var shader = ResourceLoader.Load<Shader>("res://shaders/seabed_blend.gdshader");
        if (shader is null) return false;
        var material = new ShaderMaterial { Shader = shader, ResourceName = "P2 Blended Seabed" };
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
        // Unequal, wandering contours expose five broad depth bands along the basin.
        // Each transition is a slope that remains part of the same triangulated mesh.
        var deepToLower = z - 2.0f * Mathf.Sin(0.14f * x + 0.3f);
        var lowerToMiddle = z + 2.5f * Mathf.Sin(0.11f * x - 0.7f);
        var middleToUpper = z - 2.1f * Mathf.Sin(0.17f * x + 1.4f);
        var upperToShallow = z + 2.8f * Mathf.Sin(0.10f * x + 0.6f);
        var shelves = -9.4f
            + 3.3f * Smooth(-25.0f, -18.0f, deepToLower)
            + 3.2f * Smooth(-11.0f, -4.0f, lowerToMiddle)
            + 3.1f * Smooth(3.0f, 10.0f, middleToUpper)
            + 3.2f * Smooth(17.0f, 24.0f, upperToShallow);
        var folds = 0.30f * Mathf.Sin(0.19f * x + 0.13f * z) * Mathf.Sin(0.23f * z)
            + 0.18f * Mathf.Sin(0.37f * x - 0.16f * z);
        // Local shoulders and a channel break the contour continuity without holes.
        var westernShoulder = 1.0f * Ellipse(x, z, -23.0f, -3.0f, 12.0f, 17.0f);
        var easternChannel = -0.9f * Ellipse(x, z, 23.0f, 7.0f, 10.0f, 20.0f);
        // The irregular outer rise meets the collision-only side limits above play.
        var edge = Mathf.Max(Mathf.Abs(x + 1.5f * Mathf.Sin(0.11f * z)),
            Mathf.Abs(z + 1.2f * Mathf.Sin(0.13f * x)));
        var rim = 19.0f * Smooth(31.0f, 39.0f, edge);
        return shelves + folds + westernShoulder + easternChannel + rim;
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
