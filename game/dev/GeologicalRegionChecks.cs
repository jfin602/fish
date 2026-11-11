using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Godot;

namespace Fish;

public static class GeologicalRegionChecks
{
    public static void Run(Main main)
    {
        var seabed = main.GetNode<SeabedSurface>("Habitat/Seabed");
        var geology = main.GetNode<FafGeologyWorld>("Habitat/Composition/ReefComposition/Geology");
        var regions = geology.GeneratedRegions;
        Require(regions.Count == 5 && regions.Select(r => r.Id).Distinct().Count() == 5,
            "five distinct generated structures required");
        Require(regions.Count(r => r.Kind == "cave") == 1 &&
            regions.Count(r => r.Kind == "tunnel") == 2 &&
            regions.Count(r => r.Kind == "arch") == 1 &&
            regions.Count(r => r.Kind == "canyon") == 1, "structure inventory differs");
        var same = GeologicalRegionGenerator.Generate(new GeneratedBasin(seabed.Basin.Seed));
        var other = GeologicalRegionGenerator.Generate(new GeneratedBasin(seabed.Basin.Seed + 1));
        var fingerprints = new HashSet<string>();
        var triangleTotal = 0;
        for (var i = 0; i < regions.Count; i++)
        {
            var region = regions[i];
            var body = geology.GetNode<StaticBody3D>($"GeneratedRegions/{region.Id}");
            var visual = body.GetNode<MeshInstance3D>("Mesh");
            var collisionNode = body.GetNode<CollisionShape3D>("Collision");
            var line = body.GetNode<Node3D>("Centerline");
            Require(ReferenceEquals(visual.Mesh, region.Mesh) &&
                ReferenceEquals(collisionNode.Shape, region.Collision) &&
                body.CollisionLayer == 1 && body.CollisionMask == 0 &&
                line.GetChildCount() == region.Centerline.Length, $"{region.Id} scene wiring");
            Require(line.GetNode<Marker3D>("Entry").Position.DistanceTo(region.Centerline[0]) < .001f &&
                line.GetNode<Marker3D>("Exit").Position.DistanceTo(region.Centerline[^1]) < .001f &&
                line.GetNode<Marker3D>("Center").Position.DistanceTo(region.Centerline[region.Centerline.Length / 2]) < .001f,
                $"{region.Id} route hooks");
            foreach (var child in line.GetChildren())
            {
                var marker = (Marker3D)child;
                Require(Mathf.Abs(marker.GetMeta("fish_probe_radius").AsSingle() - .48f) < .001f &&
                    Mathf.Abs(marker.GetMeta("camera_probe_radius").AsSingle() - .22f) < .001f &&
                    marker.HasMeta("nominal_fish_side_room") &&
                    (region.NominalCameraHeadroom is null ? marker.HasMeta("open_top") :
                        marker.HasMeta("nominal_camera_headroom")),
                    $"{region.Id} clearance probe metadata");
            }
            var arrays = region.Mesh.SurfaceGetArrays(0);
            var vertices = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            var normals = arrays[(int)Mesh.ArrayType.Normal].AsVector3Array();
            Require(vertices.Length > 0 && vertices.Length % 3 == 0 &&
                vertices.Length == normals.Length && region.Collision.BackfaceCollision &&
                region.Collision.Data.Length == vertices.Length, $"{region.Id} mesh/collision inventory");
            var edges = new Dictionary<string, int>();
            var orientation = new Dictionary<string, int>();
            var buried = 0;
            var floors = 0;
            var walls = 0;
            var ceilings = 0;
            for (var v = 0; v < vertices.Length; v += 3)
            {
                var a = vertices[v];
                var b = vertices[v + 1];
                var c = vertices[v + 2];
                Require(Finite(a) && Finite(b) && Finite(c) &&
                    (b - a).Cross(c - a).Length() > .0001f, $"{region.Id} degenerate/nonfinite triangle {v / 3}");
                AddEdge(edges, orientation, a, b);
                AddEdge(edges, orientation, b, c);
                AddEdge(edges, orientation, c, a);
                for (var j = v; j < v + 3; j++)
                {
                    Require(vertices[j].DistanceTo(region.Collision.Data[j]) < .0001f,
                        $"{region.Id} collision differs from visual mesh");
                    if (vertices[j].Y < seabed.GetHeightAt(vertices[j].X, vertices[j].Z) - 1.2f)
                        buried++;
                }
                var normal = (normals[v] + normals[v + 1] + normals[v + 2]).Normalized();
                if (normal.Y > .65f) floors++;
                else if (normal.Y < -.65f) ceilings++;
                else walls++;
            }
            Require(edges.Values.All(count => count == 2), $"{region.Id} mesh edge is not paired");
            var misoriented = orientation.FirstOrDefault(pair => pair.Value != 0);
            Require(misoriented.Key is null,
                $"{region.Id} mesh edge orientation mismatch {misoriented.Key} sum={misoriented.Value}");
            Require(buried > 40, $"{region.Id} lacks broad buried basin overlap");
            Require(floors > 0 && walls > 0 && ceilings > 0, $"{region.Id} surface classes absent");
            if (region.Kind != "canyon")
            {
                var center = region.Centerline[region.Centerline.Length / 2];
                var ys = vertices.Where(p => new Vector2(p.X - center.X, p.Z - center.Z).Length() < .03f)
                    .Select(p => p.Y).ToArray();
                Require(ys.Length >= 2 && ys.Max() - ys.Min() > 4.8f,
                    $"{region.Id} lacks two surfaces at same X/Z");
                Require(region.NominalFishSideRoom > 2f && region.NominalCameraHeadroom > 1.5f,
                    $"{region.Id} nominal shell width/headroom");
            }
            var hash = Hash(vertices);
            Require(hash == Hash(same[i].Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array()),
                $"{region.Id} same-seed nondeterminism");
            Require(hash != Hash(other[i].Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array()),
                $"{region.Id} seed does not affect geometry");
            Require(fingerprints.Add(hash), $"{region.Id} duplicates another structure");
            triangleTotal += vertices.Length / 3;
            GD.Print($"GEOLOGY_REGION id={region.Id} kind={region.Kind} triangles={vertices.Length / 3} " +
                $"collision_triangles={region.Collision.Data.Length / 3} paired_edges={edges.Count} " +
                $"buried_vertices={buried} floor={floors} wall={walls} ceiling={ceilings} " +
                $"nominal_fish_side_room={region.NominalFishSideRoom:F2} " +
                $"nominal_camera_headroom={region.NominalCameraHeadroom?.ToString("F2") ?? "open"}");
        }
        GD.Print($"GEOLOGY_MESH_OK structures={regions.Count} triangles={triangleTotal} " +
            "manifold=paired same_seed=true different_seed=true collision=identical routes=named");
    }

    private static bool Finite(Vector3 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);

    private static void AddEdge(Dictionary<string, int> edges, Dictionary<string, int> orientation,
        Vector3 a, Vector3 b)
    {
        var ka = Key(a);
        var kb = Key(b);
        var key = string.CompareOrdinal(ka, kb) < 0 ? ka + ":" + kb : kb + ":" + ka;
        edges[key] = edges.GetValueOrDefault(key) + 1;
        orientation[key] = orientation.GetValueOrDefault(key) +
            (string.CompareOrdinal(ka, kb) < 0 ? 1 : -1);
    }

    private static string Key(Vector3 p) => $"{Mathf.RoundToInt(p.X * 1000)}:{Mathf.RoundToInt(p.Y * 1000)}:{Mathf.RoundToInt(p.Z * 1000)}";

    private static string Hash(Vector3[] vertices)
    {
        var bytes = new byte[vertices.Length * 12];
        for (var i = 0; i < vertices.Length; i++)
        {
            BitConverter.GetBytes(vertices[i].X).CopyTo(bytes, i * 12);
            BitConverter.GetBytes(vertices[i].Y).CopyTo(bytes, i * 12 + 4);
            BitConverter.GetBytes(vertices[i].Z).CopyTo(bytes, i * 12 + 8);
        }
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new Exception(reason);
    }
}
