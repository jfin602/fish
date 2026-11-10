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
        var builders = new SurfaceTool[materials.Length];
        for (var index = 0; index < builders.Length; index++)
        {
            builders[index] = new SurfaceTool();
            builders[index].Begin(Mesh.PrimitiveType.Triangles);
            builders[index].SetMaterial(materials[index]);
        }

        for (var z = 0; z < CellsPerSide; z++)
        for (var x = 0; x < CellsPerSide; x++)
        {
            var left = -HalfExtent + x * CellSize;
            var near = -HalfExtent + z * CellSize;
            var right = left + CellSize;
            var far = near + CellSize;
            var builder = builders[MaterialAt((left + right) * 0.5f, (near + far) * 0.5f)];
            var a = Point(left, near);
            var b = Point(left, far);
            var c = Point(right, near);
            var d = Point(right, far);
            // Winding faces upward; both triangles share the same analytic corner heights.
            AddTriangle(builder, a, c, b);
            AddTriangle(builder, c, d, b);
        }

        var mesh = new ArrayMesh();
        foreach (var builder in builders)
        {
            builder.Commit(mesh);
            builder.Dispose();
        }
        var shape = mesh.CreateTrimeshShape() as ConcavePolygonShape3D;
        if (shape is null) return false;
        shape.BackfaceCollision = true;
        GetNode<MeshInstance3D>("Mesh").Mesh = mesh;
        GetNode<CollisionShape3D>("Collision").Shape = shape;
        return true;
    }

    public float GetHeightAt(float x, float z)
    {
        var radius = Mathf.Sqrt(x * x + z * z);
        var gentle = Smooth(2.5f, 7.0f, radius);
        var rolling = 0.55f * Mathf.Sin(0.27f * x) * Mathf.Sin(0.33f * z) +
            0.3f * Mathf.Sin(0.63f * x + 0.37f * z);
        var rim = 18.0f * Smooth(22.0f, 39.0f, radius);
        return -5.6f + gentle * rolling + rim;
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

    private static int MaterialAt(float x, float z)
    {
        var radius = Mathf.Sqrt(x * x + z * z);
        if (radius > 21.2f + 1.4f * Mathf.Sin(0.31f * x + 0.19f * z))
            return HabitatPalette.ExposedRock;
        if (z > 3.5f + 1.8f * Mathf.Sin(0.35f * x))
            return HabitatPalette.Gravel;
        if (x > 2.8f + 1.3f * Mathf.Sin(0.4f * z))
            return HabitatPalette.ShellSand;
        return HabitatPalette.CoastSand;
    }

    private static float Smooth(float start, float end, float value)
    {
        var t = Mathf.Clamp((value - start) / (end - start), 0, 1);
        return t * t * (3 - 2 * t);
    }
}
