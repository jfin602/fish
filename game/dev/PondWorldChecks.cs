#nullable enable
using Fish;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class PondWorldChecks
{
    public static async System.Threading.Tasks.Task Run(Main main, string directory)
    {
        var habitat = main.GetNode<Node3D>("Habitat");
        var fish = main.GetNode<FishController>("Fish");
        var before = WorldGenerationAudit.StageInvocations;
        var (manifest, data) = FullWorldPackage.Load(directory, habitat, WorldCatalog.PondId);
        await main.ToSignal(main.GetTree(), SceneTree.SignalName.PhysicsFrame);
        await main.ToSignal(main.GetTree(), SceneTree.SignalName.PhysicsFrame);
        Require(WorldGenerationAudit.StageInvocations == before, "generator ran on baked load");
        var faf = habitat.GetNode<Node3D>("PondFafEnvironment");
        var catalog = PondFafEnvironment.LoadCatalog();
        var bakedBasin = habitat.GetNode<SeabedSurface>("Seabed").Basin;
        var firstPlan = PondFafEnvironment.Plan(data, bakedBasin, catalog);
        var secondPlan = PondFafEnvironment.Plan(data, bakedBasin, catalog);
        Require(firstPlan.SequenceEqual(secondPlan) && firstPlan.Select(p => p.Id).Distinct().Count() == 33 &&
            (int)faf.GetMeta("placement_count") == firstPlan.Count,
            "Pond FAF placement identity or determinism failed");
        Require(firstPlan.Where(p => p.Zone is "lily_pad" or "floating_muck")
            .All(p => Mathf.Abs(p.Position.Y - GeneratedBasin.WaterSurfaceY) < .001f) &&
            firstPlan.Where(p => p.Zone is "cattail" or "reed" or "submerged_algae")
                .All(p => p.Position.Y < GeneratedBasin.WaterSurfaceY),
            "Pond FAF waterline or rooted placement failed");
        Require(habitat.GetNode<MeshInstance3D>("Seabed/Mesh").MaterialOverride is ShaderMaterial
            { ResourceName: "Pond silt/mud basin" }, "Pond FAF mud/silt palette missing");
        Require(faf.GetChildren().OfType<Node3D>().Count() > 0 &&
            !faf.GetChildren().Any(n => n.Name.ToString().Contains("FAF_CORAL")),
            "Pond FAF habitat restriction failed");
        var hero = faf.GetChildren().OfType<Node3D>()
            .Single(n => n.Name.ToString().StartsWith("FAF_WOOD_002A_", StringComparison.Ordinal));
        Require(hero.GetNode<StaticBody3D>("SimpleWoodCollision").GetChildCount() == 16,
            "hero log collision lacks open segmented inner passage");
        var space = habitat.GetWorld3D().DirectSpaceState;
        var staticBodies = faf.GetChildren().OfType<Node3D>()
            .SelectMany(n => n.GetChildren().OfType<StaticBody3D>()).ToArray();
        var excluded = new Godot.Collections.Array<Rid> { fish.GetRid(),
            habitat.GetNode<StaticBody3D>("HiddenLimits").GetRid() };
        foreach (var body in staticBodies) excluded.Add(body.GetRid());
        var supportMax = 0f;
        foreach (var piece in faf.GetChildren().OfType<Node3D>())
        {
            var asset = catalog.Single(a => piece.Name.ToString().StartsWith(
                a.Id.Replace('-', '_') + "_", StringComparison.Ordinal));
            var id = asset.Id;
            var halfX = asset.Size[0] * .35f;
            var halfZ = asset.Size[2] * .35f;
            foreach (var offset in new[] { Vector3.Zero,
                new Vector3(halfX, 0, halfZ), new Vector3(-halfX, 0, halfZ),
                new Vector3(halfX, 0, -halfZ), new Vector3(-halfX, 0, -halfZ) })
            {
                var site = piece.ToGlobal(offset);
                var ray = PhysicsRayQueryParameters3D.Create(site + Vector3.Up * 15f,
                    site + Vector3.Down * 40f, 1, excluded);
                ray.HitBackFaces = true;
                var hit = space.IntersectRay(ray);
                Require(hit.Count > 0, $"unsupported Pond structure {id} site={site} offset={offset}");
                var y = hit["position"].AsVector3().Y;
                supportMax = Mathf.Max(supportMax, Mathf.Abs(y - piece.GlobalPosition.Y));
            }
        }
        Require(supportMax < 1.5f, $"Pond structures lack terrain support: {supportMax:F3}");
        var fishQuery = new PhysicsShapeQueryParameters3D
        {
            Shape = fish.GetNode<CollisionShape3D>("Collision").Shape,
            CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { fish.GetRid() },
        };
        var cameraQuery = new PhysicsShapeQueryParameters3D
        {
            Shape = new SphereShape3D { Radius = GeneratedWorldRoutes.CameraRadius },
            CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { fish.GetRid() },
        };
        for (var z = -5.4f; z <= 5.4f; z += .3f)
        {
            var center = hero.ToGlobal(new Vector3(0, 2.52f, z));
            fishQuery.Transform = new Transform3D(Basis.Identity, center);
            cameraQuery.Transform = new Transform3D(Basis.Identity, center + Vector3.Up * 1.15f);
            Require(space.IntersectShape(fishQuery, 1).Count == 0,
                $"hero fish passage blocked at z={z:F2}");
            Require(space.IntersectShape(cameraQuery, 1).Count == 0,
                $"hero camera passage blocked at z={z:F2}");
        }
        GD.Print($"POND_FAF_CHECK assets=33 placements={firstPlan.Count} hero={hero.GlobalPosition} " +
            $"hero_yaw={hero.Rotation.Y:F3} fish_camera_probes=74 support_max={supportMax:F3} ocean_assets=0");
        var seabed = habitat.GetNode<SeabedSurface>("Seabed");
        var basin = seabed.Basin;
        Require(basin.IsPond && manifest.BasinAlgorithm == basin.AlgorithmIdentity,
            "wrong basin algorithm");
        Require(seabed.GeneratedMesh?.GetSurfaceCount() == 1 &&
            seabed.GeneratedCollision is { Data.Length: > 110000 and < 120000 },
            "simplified terrain collision missing");
        var collisionVertices = seabed.GeneratedCollision!.Data;
        foreach (var vertex in collisionVertices)
            Require(Mathf.Abs(vertex.Y - basin.HeightAt(vertex.X, vertex.Z)) < .0002f,
                "collision does not follow terrain height field");
        var maxSimplificationError = 0f;
        var step = GeneratedBasin.HalfExtent * 2f / 138f;
        for (var iz = 0; iz < 138; iz++)
        for (var ix = 0; ix < 138; ix++)
        {
            var x = -GeneratedBasin.HalfExtent + ix * step;
            var z = -GeneratedBasin.HalfExtent + iz * step;
            var middleX = x + step * .5f; var middleZ = z + step * .5f;
            if (basin.DistanceToShore(middleX, middleZ) < 0f) continue;
            var physical = .5f * (basin.HeightAt(x, z + step) + basin.HeightAt(x + step, z));
            maxSimplificationError = Mathf.Max(maxSimplificationError,
                Mathf.Abs(physical - basin.HeightAt(middleX, middleZ)));
        }
        Require(maxSimplificationError < .7f,
            $"visual/collision terrain gap {maxSimplificationError:F3} m");
        var required = new[] { "spawn", "shallow", "middle", "deep", "return", "alternate" };
        Require(required.All(id => data.Nodes.Count(n => n.Id == id) == 1), "route landmarks missing");
        Require(habitat.GetNode<Node3D>("PondLandmarks").GetChildCount() == 6 &&
            habitat.GetNode<Marker3D>("PondLandmarks/BankApproach").Position.Y >
                GeneratedBasin.WaterSurfaceY,
            "inspectable markers missing");
        var nodes = data.Nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);
        var shallow = Depth("shallow");
        var middle = Depth("middle");
        var deep = Depth("deep");
        Require(shallow < middle && middle < deep && deep > 8f,
            "shore-to-core depth sequence invalid");
        Require(basin.DistanceToShore(nodes["spawn"].Position[0], nodes["spawn"].Position[2]) > 5f,
            "spawn too close to bank");
        Require(basin.HeightAt(basin.ShoreMax.X + 25f, 0f) > GeneratedBasin.WaterSurfaceY,
            "bank does not rise above water");
        var graph = data.Edges.GroupBy(e => e.From)
            .ToDictionary(g => g.Key, g => g.Select(e => e.To).ToArray());
        Require(Reach("spawn", "shallow", "middle", "deep", "return", "shallow") &&
            Reach("shallow", "middle", "alternate", "shallow"), "required loop/alternate missing");
        using var planner = new WorldRoutePlanner(habitat.GetWorld3D().DirectSpaceState,
            fish.GetRid(), basin, Array.Empty<GeologicalRegion>());
        var probes = 0;
        foreach (var edge in data.Edges)
        {
            Require(edge.Points.Length >= 2 && nodes.ContainsKey(edge.From) && nodes.ContainsKey(edge.To),
                "route topology invalid");
            for (var segment = 1; segment < edge.Points.Length; segment++)
            {
                var a = Vec(edge.Points[segment - 1]);
                var b = Vec(edge.Points[segment]);
                var steps = Mathf.Max(1, Mathf.CeilToInt(a.DistanceTo(b) / .3f));
                for (var i = 0; i <= steps; i++)
                {
                    var point = a.Lerp(b, i / (float)steps);
                    Require(planner.PointClear(point), $"blocked {edge.Id} at {point}");
                    probes++;
                }
            }
        }
        Require(data.Environment.All(row => row.All(float.IsFinite)), "nonfinite environment field");
        GD.Print($"POND_WORLD_CHECK_OK id={manifest.WorldId} seed={manifest.Seed} content={manifest.ContentSha256} shallow={shallow:F2} middle={middle:F2} deep={deep:F2} probes={probes} collision_vertices={collisionVertices.Length} max_gap={maxSimplificationError:F3} generator_invocations=0");

        float Depth(string id) => GeneratedBasin.WaterSurfaceY - basin.HeightAt(
            nodes[id].Position[0], nodes[id].Position[2]);
        bool Reach(params string[] path)
        {
            for (var i = 1; i < path.Length; i++)
                if (!graph.TryGetValue(path[i - 1], out var destinations) ||
                    !destinations.Contains(path[i], StringComparer.Ordinal)) return false;
            return true;
        }
    }

    private static Vector3 Vec(float[] p) => new(p[0], p[1], p[2]);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
