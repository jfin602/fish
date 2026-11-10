#nullable enable
using Godot;
using System.Collections.Generic;

namespace Fish;

/// <summary>Fixed P3 geological formations over the P2 floor and route authority.</summary>
public partial class FafGeologyWorld : Node3D
{
    private readonly record struct Piece(string Name, char Family, char Variant, float X, float Z,
        float Lift, float Yaw, Vector3 Scale, string Formation);

    // Shelf/drop-off pieces carry the silhouette. Rocks interlock along their edges;
    // no generated boulder supplies a single lintel across a passage.
    private static readonly Piece[] Layout =
    {
        new("WestShelf", 'T', 'C', -16.5f, -13.5f, 1.0f, .27f, new(2.15f, 2.5f, 2.0f), "west-tunnel"),
        new("WestBreak", 'T', 'E', -22.0f, -14.5f, .5f, -.42f, new(1.75f, 2.2f, 1.65f), "west-tunnel"),
        new("WestCrown", 'T', 'F', -8.2f, -13.5f, 6.0f, .19f, new(2.0f, 1.5f, 1.75f), "west-tunnel"),
        new("CaveShelf", 'T', 'C', 14.8f, -14.2f, 1.0f, -.35f, new(1.65f, 2.3f, 1.85f), "east-cave"),
        new("CaveCeiling", 'T', 'F', 9.2f, -13.4f, 7.4f, .38f, new(1.9f, 1.4f, 1.8f), "east-cave"),
        new("CaveBreak", 'T', 'E', 15.8f, -18.8f, .2f, .62f, new(1.6f, 2.0f, 1.65f), "east-cave"),
        new("CanyonWest", 'T', 'E', -15.1f, -24.0f, .3f, -.2f, new(1.65f, 2.7f, 2.25f), "deep-corridor"),
        new("CanyonEast", 'T', 'C', -1.8f, -25.3f, 5.5f, .48f, new(1.55f, 2.5f, 1.85f), "deep-corridor"),
        new("NorthShelf", 'T', 'C', -22.8f, 11.5f, .3f, .57f, new(1.8f, 2.0f, 1.65f), "shelf-transition"),
        new("EastOverhang", 'T', 'E', 28.0f, 4.5f, 3.4f, -.31f, new(1.7f, 1.8f, 1.7f), "shelf-transition"),
        new("DeepSpur", 'T', 'F', -19.7f, -22.5f, 2.5f, .33f, new(1.7f, 1.8f, 1.7f), "deep-spur"),
        new("ShallowBeacon", 'T', 'C', 29.0f, 18.0f, .6f, -.65f, new(1.8f, 2.5f, 1.65f), "shallow-landmark"),

        new("WestJawA", 'R', 'C', -15.4f, -8.8f, 0, .2f, new(2.2f, 2.6f, 2.3f), "west-tunnel"),
        new("WestJawB", 'R', 'D', -1.9f, -10.5f, 0, -.4f, new(2.1f, 2.8f, 2.4f), "west-tunnel"),
        new("WestJawC", 'R', 'B', -14.8f, -17.8f, 0, .75f, new(2.0f, 2.0f, 1.8f), "west-tunnel"),
        new("WestJawD", 'R', 'A', -1.6f, -18.0f, 0, -.7f, new(2.1f, 2.4f, 2.2f), "west-tunnel"),
        new("WestFacet", 'R', 'D', -18.3f, -14.1f, 0, .9f, new(2.0f, 2.0f, 1.9f), "west-tunnel"),
        new("WestCrownFacet", 'R', 'B', -9.0f, -13.7f, 7.0f, .44f, new(2.4f, 1.5f, 2.0f), "west-tunnel"),
        new("CaveShoulderA", 'R', 'C', 14.6f, -9.5f, 0, -.5f, new(2.1f, 2.7f, 2.1f), "east-cave"),
        new("CaveShoulderB", 'R', 'D', 14.3f, -18.5f, 0, .8f, new(2.1f, 2.5f, 2.2f), "east-cave"),
        new("CaveEave", 'R', 'B', 10.8f, -13.8f, 7.2f, -.2f, new(2.8f, 1.4f, 2.2f), "east-cave"),
        new("CaveEdgeA", 'R', 'A', 11.9f, -18.5f, 0, .6f, new(1.6f, 1.8f, 1.7f), "east-cave"),
        new("CaveEdgeB", 'R', 'A', 13.0f, -7.8f, 0, -.5f, new(1.5f, 1.9f, 1.6f), "east-cave"),
        new("CanyonWallA", 'R', 'C', -13.6f, -22.5f, 0, .2f, new(2.5f, 3.0f, 2.7f), "deep-corridor"),
        new("CanyonWallB", 'R', 'D', -13.7f, -26.3f, 0, -.3f, new(2.4f, 2.8f, 2.5f), "deep-corridor"),
        new("CanyonWallC", 'R', 'C', -3.3f, -20.5f, 0, .6f, new(2.7f, 2.9f, 2.6f), "deep-corridor"),
        new("CanyonWallD", 'R', 'B', -3.6f, -28.4f, 0, -.7f, new(2.6f, 3.0f, 1.5f), "deep-corridor"),
        new("CanyonRim", 'R', 'A', -15.5f, -26.8f, 4.0f, .7f, new(2.1f, 1.8f, 1.8f), "deep-corridor"),
        new("NorthFacetA", 'R', 'D', -22.3f, 8.1f, 0, .4f, new(2.5f, 2.2f, 2.0f), "shelf-transition"),
        new("NorthFacetB", 'R', 'B', -19.3f, 14.9f, 0, -.8f, new(2.0f, 2.0f, 2.0f), "shelf-transition"),
        new("EastFacetA", 'R', 'C', 28.0f, 1.8f, 0, -.3f, new(2.2f, 2.5f, 2.1f), "shelf-transition"),
        new("EastFacetB", 'R', 'A', 29.0f, 7.4f, 0, .7f, new(2.0f, 2.1f, 1.9f), "shelf-transition"),
        new("DeepSpurA", 'R', 'D', -20.5f, -22.6f, 0, .2f, new(3.0f, 3.1f, 2.8f), "deep-spur"),
        new("DeepSpurB", 'R', 'C', -22.6f, -25.8f, 0, -.7f, new(2.4f, 2.8f, 2.2f), "deep-spur"),
        new("DeepSpurC", 'R', 'B', -18.4f, -26.1f, 0, .8f, new(2.0f, 2.0f, 2.0f), "deep-spur"),
        new("ShallowBeaconA", 'R', 'C', 27.0f, 18.5f, 0, .3f, new(2.5f, 3.2f, 2.4f), "shallow-landmark"),
        new("ShallowBeaconB", 'R', 'D', 30.0f, 16.5f, 0, -.4f, new(2.4f, 2.8f, 2.2f), "shallow-landmark"),
        new("ShallowBeaconC", 'R', 'A', 29.5f, 21.2f, 0, .8f, new(2.0f, 2.1f, 1.8f), "shallow-landmark"),
    };

    public int PieceCount => Layout.Length;

    public bool TryBuild(SeabedSurface seabed, out string diagnostic)
    {
        var visuals = GetNodeOrNull<Node3D>("Visuals");
        var collisions = GetNodeOrNull<Node3D>("Collision");
        var routes = GetNodeOrNull<Node3D>("Routes");
        if (visuals is null || collisions is null || routes is null || visuals.GetChildCount() != 0)
        {
            diagnostic = "FAF_GEOLOGY_INVALID action=check-authored-roots";
            return false;
        }
        foreach (var piece in Layout)
        {
            var id = piece.Family == 'T' ? $"FAF-TERRAIN-001{piece.Variant}" : $"FAF-ROCK-001{piece.Variant}";
            var wrapper = FafRuntimeCatalog.GetEnvironment(id).CreateVisual();
            wrapper.Name = piece.Name;
            wrapper.Position = new Vector3(piece.X, seabed.GetHeightAt(piece.X, piece.Z) + piece.Lift, piece.Z);
            wrapper.Rotation = new Vector3(0, piece.Yaw, 0);
            wrapper.Scale = piece.Scale;
            wrapper.SetMeta("formation", piece.Formation);
            visuals.AddChild(wrapper);
        }
        // Only these route-defining solids affect physics. The west tunnel and
        // original shelter retain their accepted P2 simplified collision.
        AddBox(collisions, seabed, "CanyonWestWall", -14.5f, -22.2f, 3.4f, 5.0f, 4.0f);
        AddBox(collisions, seabed, "CanyonEastWall", -3.5f, -21.5f, 3.1f, 5.0f, 2.5f);
        AddBox(collisions, seabed, "CaveOuterLip", 14.0f, -17.6f, 2.2f, 4.2f, 2.0f);
        foreach (var child in routes.GetChildren())
        {
            if (child is not Marker3D marker) continue;
            var lift = marker.GetMeta("height_above_floor", 2.0f).AsSingle();
            marker.Position = new Vector3(marker.Position.X,
                seabed.GetHeightAt(marker.Position.X, marker.Position.Z) + lift,
                marker.Position.Z);
        }
        diagnostic = "";
        GD.Print($"FAF_GEOLOGY_READY pieces={PieceCount} collision=3 routes=west-tunnel,east-cave,deep-corridor");
        return true;
    }

    private static void AddBox(Node3D root, SeabedSurface seabed, string name,
        float x, float z, float width, float height, float depth)
    {
        var body = new StaticBody3D { Name = name, CollisionLayer = 1, CollisionMask = 0,
            Position = new Vector3(x, seabed.GetHeightAt(x, z), z) };
        body.AddChild(new CollisionShape3D { Name = "Shape",
            Position = new Vector3(0, height / 2, 0),
            Shape = new BoxShape3D { Size = new Vector3(width, height, depth) } });
        root.AddChild(body);
    }
}
