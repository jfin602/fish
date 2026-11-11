#nullable enable
using Godot;

namespace Fish;

/// <summary>One generated open seabed; visual triangles and two-sided collision share a mesh.</summary>
public partial class SeabedSurface : StaticBody3D
{
    public const float HalfExtent = GeneratedBasin.HalfExtent;
    public const float CellSize = GeneratedBasin.CellSize;
    public const int CellsPerSide = GeneratedBasin.CellsPerSide;
    public GeneratedBasin Basin { get; private set; } = new(GeneratedBasin.DefaultSeed);
    public ArrayMesh? GeneratedMesh => GetNode<MeshInstance3D>("Mesh").Mesh as ArrayMesh;
    public ConcavePolygonShape3D? GeneratedCollision => GetNode<CollisionShape3D>("Collision").Shape as ConcavePolygonShape3D;
    public Vector2 DeepCore => Basin.DeepCore;
    public float DeepFloorY => Basin.DeepFloorY;
    public float DeepReferenceY => DeepFloorY + 2f;
    /// <summary>The generated Pond's level water plane, available only after its baked geometry is loaded.</summary>
    public bool TryGetPondWaterSurfaceHeight(out float height)
    {
        height = 0f;
        if (!Basin.IsPond || GeneratedMesh is null || GeneratedCollision is null) return false;
        height = ToGlobal(new Vector3(0f, GeneratedBasin.WaterSurfaceY, 0f)).Y;
        return float.IsFinite(height) && GlobalTransform.Basis.Y.IsEqualApprox(Vector3.Up);
    }
    private ShaderMaterial? _waterMaterial;

    public void SetBasinSeed(ulong seed, bool pond = false)
    {
        if (GeneratedMesh is not null)
            throw new System.InvalidOperationException("Basin seed must be selected before mesh construction");
        Basin = new GeneratedBasin(seed, pond: pond);
    }

    public void RestoreBakedSeed(ulong seed, bool legacy, bool m1Baked = false, bool pond = false)
    {
        Basin = new GeneratedBasin(seed, legacy, m1Baked, pond);
        ConfigureLimits(legacy);
    }

    public void InstallBaked(ArrayMesh mesh, ConcavePolygonShape3D collision, ulong seed, bool legacy,
        bool m1Baked = false, bool pond = false)
    {
        RestoreBakedSeed(seed, legacy, m1Baked, pond);
        GetNode<MeshInstance3D>("Mesh").Mesh = mesh;
        GetNode<CollisionShape3D>("Collision").Shape = collision;
        _waterMaterial = mesh.SurfaceGetMaterial(0) as ShaderMaterial;
    }

    public void SetWaterColumn(float surfaceY, float deepY)
    {
        if (!float.IsFinite(surfaceY) || !float.IsFinite(deepY) || deepY >= surfaceY)
            throw new System.ArgumentOutOfRangeException(nameof(deepY), "Invalid water-column anchors");
        _waterMaterial?.SetShaderParameter("water_surface_y", surfaceY);
        _waterMaterial?.SetShaderParameter("water_deep_y", deepY);
    }

    public bool Build(StandardMaterial3D[] materials)
    {
        WorldGenerationAudit.MarkStage();
        ConfigureLimits(false);
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
        var shape = Basin.IsPond ? BuildPondCollision() :
            mesh.CreateTrimeshShape() as ConcavePolygonShape3D;
        if (shape is null) return false;
        shape.BackfaceCollision = true;
        GetNode<MeshInstance3D>("Mesh").Mesh = mesh;
        GetNode<CollisionShape3D>("Collision").Shape = shape;
        return true;
    }

    public float GetHeightAt(float x, float z) => Basin.HeightAt(x, z);

    private Vector3 Point(float x, float z) => new(x, GetHeightAt(x, z), z);

    private ConcavePolygonShape3D BuildPondCollision()
    {
        // Visual terrain is sampled every 2 m; physical terrain uses the same
        // analytical height field at a coarser uniform lattice.
        var vertices = new System.Collections.Generic.List<Vector3>();
        const int collisionCells = 138;
        const float collisionCell = HalfExtent * 2f / collisionCells;
        for (var z = 0; z < collisionCells; z++)
        for (var x = 0; x < collisionCells; x++)
        {
            var left = -HalfExtent + x * collisionCell;
            var near = -HalfExtent + z * collisionCell;
            var right = -HalfExtent + (x + 1) * collisionCell;
            var far = -HalfExtent + (z + 1) * collisionCell;
            var a = Point(left, near);
            var b = Point(left, far);
            var c = Point(right, near);
            var d = Point(right, far);
            vertices.Add(a); vertices.Add(c); vertices.Add(b);
            vertices.Add(c); vertices.Add(d); vertices.Add(b);
        }
        return new ConcavePolygonShape3D { Data = vertices.ToArray() };
    }

    private void AddTriangle(SurfaceTool builder, Vector3 a, Vector3 b, Vector3 c)
    {
        AddVertex(builder, a);
        AddVertex(builder, b);
        AddVertex(builder, c);
    }

    private void AddVertex(SurfaceTool builder, Vector3 point)
    {
        builder.SetNormal(Basin.NormalAt(point.X, point.Z));
        builder.SetUV(new Vector2(point.X, point.Z) * 0.24f);
        builder.AddVertex(point);
    }

    private void ConfigureLimits(bool legacy)
    {
        var limits = GetParent().GetNode<StaticBody3D>("HiddenLimits");
        var extent = legacy ? 47f : HalfExtent + 1f;
        var span = legacy ? 96f : HalfExtent * 2f + 4f;
        foreach (var (name, x, z) in new[] {
            ("West", -extent, 0f), ("East", extent, 0f),
            ("North", 0f, -extent), ("South", 0f, extent) })
        {
            var node = limits.GetNode<CollisionShape3D>(name);
            node.Position = new Vector3(x, 0f, z);
            node.Shape = new BoxShape3D { Size = new Vector3(x != 0f ? .8f : span,
                legacy ? 31f : 80f, z != 0f ? .8f : span) };
        }
        var upper = limits.GetNode<CollisionShape3D>("Upper");
        // Pond swimming is limited by the motor at the real water plane. This
        // legacy upper box remains only as a surface mesh size anchor.
        upper.Disabled = Basin.IsPond;
        upper.Position = new Vector3(0f, legacy ? 9.75f : 11.2f, 0f);
        upper.Shape = new BoxShape3D { Size = new Vector3(legacy ? 77f : span,
            .6f, legacy ? 77f : span) };
    }

}
