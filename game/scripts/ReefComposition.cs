using Godot;
using System.Collections.Generic;

namespace Fish;

/// <summary>Installs the locally prepared models at scene-authored reef anchors.</summary>
public partial class ReefComposition : Node3D
{
    private sealed record Source(string Path, Vector3 BottomCenterOffset, string GltfStem = "");

    private const string Vendor = "res://assets/vendor-local/";
    private const string P2 = Vendor + "p2-environment/";
    private static readonly Dictionary<string, Source> Sources = new()
    {
        ["rock_face_01"] = new(Vendor + "rock_face_01/rock_face_01_2k.gltf", new(-0.34315f, 0.03436f, 1.55240f), "rock_face_01"),
        ["rock_face_02"] = new(P2 + "polyhaven/rock_face_02/rock_face_02_2k.gltf", new(0.90550f, 0.13962f, 0.12484f), "rock_face_02"),
        ["coastal_cliff_01"] = new(P2 + "polyhaven/coastal_cliff_01/coastal_cliff_01_2k.gltf", new(-0.93440f, 0.65003f, 0.72245f), "coastal_cliff_01"),
        ["boulder_01"] = new(P2 + "polyhaven/boulder_01/boulder_01_2k.gltf", new(0.11209f, 0.07364f, 0.03327f), "boulder_01"),
        ["heliopora_coerulea"] = new(Vendor + "heliopora-coerulea.glb", new(-0.00532f, 0.14284f, -0.00020f)),
        ["acropora_cervicornis"] = new(P2 + "smithsonian/acropora-cervicornis/Acropora_cervicornis-150k-4096_std.glb", new(0, 0.08404f, 0)),
        ["acropora_palmata"] = new(P2 + "smithsonian/acropora-palmata/usnm_3968-100k-2048_std.glb", new(0, 0.13916f, 0)),
        ["meandrina_meandrites"] = new(P2 + "smithsonian/meandrina-meandrites/usnm_1174917-100k-2048_std.glb", new(0, 0.12828f, 0)),
        ["montipora_danae"] = new(P2 + "smithsonian/montipora-danae/usnm_307-100k-2048_std.glb", new(0, 0.04827f, 0)),
    };

    public bool TryBuild(SeabedSurface seabed, out string diagnostic)
    {
        var terrain = GetNodeOrNull<FafTerrainWorld>("Terrain");
        if (terrain is null)
        {
            diagnostic = "FAF_TERRAIN_INVALID action=missing-terrain-node";
            return false;
        }
        if (!terrain.TryBuild(seabed, out diagnostic)) return false;
        var loaded = new Dictionary<string, PackedScene>();
        foreach (var (id, source) in Sources)
        {
            if (!CheckFiles(id, source, out diagnostic)) return false;
            var packed = ResourceLoader.Load<PackedScene>(source.Path);
            if (packed is null)
            {
                diagnostic = $"FISH_ASSET_INVALID item={id} path={source.Path} action=run-tools-dev-import";
                return false;
            }
            var probe = packed.Instantiate<Node3D>();
            var hasVisual = HasPreparedMesh(probe);
            probe.Free();
            if (!hasVisual)
            {
                diagnostic = $"FISH_ASSET_INVALID item={id} path={source.Path} action=check-mesh-and-imported-material";
                return false;
            }
            loaded[id] = packed;
        }

        var counts = new Dictionary<string, int>();
        foreach (var sectionName in new[] { "Rocks", "Corals" })
        {
            var section = GetNodeOrNull<Node3D>(sectionName);
            if (section is null)
            {
                diagnostic = $"FISH_REEF_INVALID section={sectionName} action=check-reef-layout";
                return false;
            }
            foreach (var child in section.GetChildren())
            {
                if (child is not Node3D piece || !piece.HasMeta("source_id"))
                {
                    diagnostic = $"FISH_REEF_INVALID section={sectionName} action=check-source-id-on-every-anchor";
                    return false;
                }
                var id = piece.GetMeta("source_id").AsString();
                if (!loaded.TryGetValue(id, out var packed))
                {
                    diagnostic = $"FISH_REEF_INVALID item={id} action=use-prepared-source-id";
                    return false;
                }
                if ((sectionName == "Rocks") != (piece is StaticBody3D) ||
                    piece.GetNodeOrNull<Node3D>("Visual") is not { } visual || visual.GetChildCount() != 0)
                {
                    diagnostic = $"FISH_REEF_INVALID item={id} anchor={piece.Name} action=check-visual-and-collision-owner";
                    return false;
                }
                if (piece is StaticBody3D rock && (rock.CollisionLayer != 1 ||
                    rock.GetNodeOrNull<CollisionShape3D>("Collision")?.Shape is null))
                {
                    diagnostic = $"FISH_REEF_INVALID item={id} anchor={piece.Name} action=check-simplified-rock-collision";
                    return false;
                }
                var height = piece.HasMeta("height_above_floor")
                    ? piece.GetMeta("height_above_floor").AsSingle() : 0.0f;
                piece.Position = new Vector3(piece.Position.X,
                    seabed.GetHeightAt(piece.Position.X, piece.Position.Z) + height,
                    piece.Position.Z);
                var model = packed.Instantiate<Node3D>();
                visual.AddChild(model);
                model.Name = "PreparedVisual";
                model.Position = Sources[id].BottomCenterOffset;
                // P2 collision and source anchors remain for the accepted routes.
                // FAF terrain, geology and dressing now own the visible reef art.
                model.Visible = false;
                counts[id] = counts.GetValueOrDefault(id) + 1;
            }
        }
        foreach (var id in Sources.Keys)
        {
            if (counts.GetValueOrDefault(id) > 0) continue;
            diagnostic = $"FISH_REEF_INVALID item={id} action=place-every-prepared-rock-and-coral";
            return false;
        }
        if (counts["coastal_cliff_01"] != 1 ||
            GetNode<Node3D>("Corals").GetChildCount() > 9)
        {
            diagnostic = "FISH_REEF_INVALID action=keep-hero-cliff-and-raw-coral-count-bounded";
            return false;
        }
        // Route markers are authored in X/Z and follow the same floor as the models.
        foreach (var child in GetNode<Node3D>("Routes").GetChildren())
        {
            if (child is not Marker3D route) continue;
            var clearance = route.HasMeta("height_above_floor")
                ? route.GetMeta("height_above_floor").AsSingle() : 2.0f;
            route.Position = new Vector3(route.Position.X,
                seabed.GetHeightAt(route.Position.X, route.Position.Z) + clearance,
                route.Position.Z);
        }
        if (GetNodeOrNull<FafGeologyWorld>("Geology") is not { } geology ||
            !geology.TryBuild(seabed, out diagnostic)) return false;
        if (GetNodeOrNull<FafReefDressing>("Dressing") is not { } dressing ||
            !dressing.TryBuild(seabed, terrain, out diagnostic)) return false;
        if (GetNodeOrNull<FafAmbientReefLife>("AmbientLife") is not { } ambient ||
            !ambient.TryBuild(seabed, out diagnostic)) return false;
        diagnostic = "";
        GD.Print($"FISH_REEF_READY rocks={GetNode<Node3D>("Rocks").GetChildCount()} corals={GetNode<Node3D>("Corals").GetChildCount()} sources={Sources.Count} cliff=1 routes=open,tunnel,shelter,vertical,depth,alternate");
        return true;
    }

    private static bool CheckFiles(string id, Source source, out string diagnostic)
    {
        var paths = new List<string> { source.Path };
        if (source.GltfStem.Length > 0)
        {
            var stem = source.GltfStem;
            var directory = source.Path[..source.Path.LastIndexOf('/')];
            paths.Add(directory + "/" + stem + ".bin");
            foreach (var kind in new[] { "arm", "diff", "nor_gl" })
                paths.Add(directory + "/textures/" + stem + "_" + kind + "_2k.jpg");
        }
        foreach (var path in paths)
        {
            if (FileAccess.FileExists(path)) continue;
            diagnostic = $"FISH_ASSET_MISSING item={id} path={path} action=restore-prepared-vendor-local-working-set";
            return false;
        }
        diagnostic = "";
        return true;
    }

    private static bool HasPreparedMesh(Node node)
    {
        if (node is MeshInstance3D { Mesh: { } mesh, Visible: true } && mesh.GetSurfaceCount() > 0)
        {
            for (var surface = 0; surface < mesh.GetSurfaceCount(); surface++)
                if (mesh.SurfaceGetMaterial(surface) is BaseMaterial3D { AlbedoTexture: not null })
                    return true;
        }
        foreach (var child in node.GetChildren())
            if (HasPreparedMesh(child)) return true;
        return false;
    }
}
