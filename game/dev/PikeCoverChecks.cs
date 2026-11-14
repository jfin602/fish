using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fish;

/// <summary>Fixed seed P5 cover, terrain, sight and physical gap integration probe.</summary>
public static class PikeCoverChecks
{
    public static async void Run(PikeEncounter scene)
    {
        var tree = scene.GetTree();
        // This legacy P2 clearance probe repositions both actors for long
        // traversal sequences; suspend the now-live Pike ecology tick.
        scene.SetPhysicsProcess(false);
        try
        {
            var catalog = PikeCoverFixture.LoadCatalog();
            var plan = scene.CoverPlan;
            var replay = PikeCoverFixture.Plan(catalog);
            Require(plan.SequenceEqual(replay), "seed replay drift");
            Require(plan.Count == 47 && plan.Select(p => p.Id).Distinct().Count() == 23,
                "incomplete cover plan");
            Require(catalog.Assets.Select(a => a.Family).Distinct().Count() == 10,
                "not ten structural families");
            Require(plan.Count(p => p.Zone == "nursery") >= 15 &&
                plan.Count(p => p.Zone == "refuge") >= 15 &&
                plan.Count(p => p.Zone == "maze") >= 4, "zones sparse");
            foreach (var placement in plan)
            {
                var expectedZone = placement.Id switch
                {
                    var id when id.StartsWith("FAF-PLANT-010") ||
                        id.StartsWith("FAF-PLANT-013") || id.StartsWith("FAF-WOOD-005") ||
                        id == "FAF-PLANT-012A" => "nursery",
                    var id when id.StartsWith("FAF-WOOD-008") ||
                        id.StartsWith("FAF-WOOD-009") => "maze",
                    _ => "refuge",
                };
                Require(placement.Zone == expectedZone,
                    "cover in wrong depth zone " + placement.Id);
                Require(Mathf.Abs(placement.Position.Y - placement.BedY + .035f) < .002f,
                    "floating root " + placement.Id);
                if (placement.Id.StartsWith("FAF-PLANT-"))
                    Require(placement.TipY <= PikeCoverFixture.WaterY + .12f,
                        "emergent vegetation " + placement.Id);
            }
            var grass = catalog.Assets.Where(a => a.Family == "ribbon_grass")
                .OrderBy(a => a.Size[2]).Select(a => a.Size[2]).ToArray();
            Require(grass.Length == 3 && grass[0] is >= .5f and <= 1f &&
                grass[1] is >= 1f and <= 2f && grass[2] >= 3.5f,
                "grass height bands");
            var cover = scene.GetNode<Node3D>("Cover");
            Require(cover.GetChildCount() == plan.Count, "missing instantiated cover");
            var expectedShapes = plan.Sum(p => catalog.Assets.Single(a => a.Id == p.Id).CollisionShapes);
            var actualShapes = cover.GetChildren().Sum(n =>
                ((Node3D)n).FindChild("AuthoredWoodCollision", true, false)?.GetChildCount() ?? 0);
            Require(expectedShapes == actualShapes && actualShapes >= 193,
                "wood wrapper collision missing");
            Require(cover.GetChildren().Where(n => n.Name.ToString().StartsWith("FAF_PLANT_"))
                .All(n => !HasCollision(n)),
                "soft plants gained hard collision");
            var supportMax = 0f;
            foreach (var placement in plan.Where(p => p.Id.StartsWith("FAF-WOOD-")))
            {
                var asset = catalog.Assets.Single(a => a.Id == placement.Id);
                var basis = new Basis(Vector3.Up, placement.Yaw);
                var support = 0f;
                foreach (var z in new[] { -asset.Size[1] * .35f, 0f, asset.Size[1] * .35f })
                {
                    var sample = placement.Position + basis * new Vector3(0, 0, z);
                    support = Mathf.Max(support,
                        Mathf.Abs(PikeCoverFixture.Bed(sample.X, sample.Z) - placement.BedY));
                }
                supportMax = Mathf.Max(supportMax, support);
                GD.Print($"PIKE_WOOD_SUPPORT id={placement.Id} x={placement.Position.X:F1} z={placement.Position.Z:F1} depth={PikeCoverFixture.WaterY-placement.BedY:F2} variation={support:F2}");
            }
            Require(supportMax < .85f, $"wood axis unsupported by mud support_max={supportMax:F2}");
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);

            var fish = scene.GetNode<FishController>("Fish");
            var pike = scene.GetNode<CharacterBody3D>("NorthernPike");
            var camera = fish.GetNode<FollowCamera>("CameraRig");
            Require(fish.GetNode<CollisionShape3D>("Collision").Shape is CapsuleShape3D,
                "Bass does not have its lengthwise physical body");
            Require(fish.GlobalPosition.DistanceTo(pike.GlobalPosition) > 15 &&
                camera.GetNode<Camera3D>("Camera3D").Current,
                "safe spawn or real follow camera absent");
            Require(SolidOnSightline(scene, fish.GlobalPosition, pike.GlobalPosition, fish, pike),
                "Bluegill spawn is exposed to Pike");
            var fishSphere = new SphereShape3D { Radius = fish.BodyRadius };
            var pikeSphere = new SphereShape3D { Radius = .72f };
            var cameraSphere = new SphereShape3D
                { Radius = .22f * Mathf.Max(.32f, fish.BodyScale) };
            // Exposed center lane is accessible to the real Pike size; both
            // outside flanks remain alternatives to the short central crossing.
            foreach (var z in new[] { 22f, 15f, 9f, 2f, -5f, -12f, -21f })
            {
                var at = new Vector3(0, PikeCoverFixture.Bed(0, z) + 1.35f, z);
                Require(!Blocked(scene, pikeSphere, at, fish, pike), "Pike lane blocked z=" + z);
                Require(!Blocked(scene, fishSphere, at, fish, pike), "fish crossing blocked z=" + z);
            }
            foreach (var x in new[] { -25f, 25f })
                foreach (var z in new[] { 18f, 1f, -17f })
                    Require(!Blocked(scene, fishSphere,
                        new Vector3(x, PikeCoverFixture.Bed(x, z) + 1.35f, z), fish, pike),
                        $"alternate lane blocked x={x} z={z}");

            // Sample the actual authored wrappers after scene placement. The
            // root undercut is a refuge; it is deliberately not a through-route.
            foreach (var id in new[] { "FAF-WOOD-006A", "FAF-WOOD-006B" })
                ProbeLateral(id, id.EndsWith("A") ? 2.3f : 2.8f, 5.6f, -4.5f, -.3f);
            foreach (var id in new[] { "FAF-WOOD-007A", "FAF-WOOD-007B" })
                ProbeLine(id, new Vector3(0, 1.4f, 6.4f),
                    new Vector3(0, 1.4f, 3.6f), "root refuge");
            foreach (var id in new[] { "FAF-WOOD-008A", "FAF-WOOD-008B" })
            {
                var length = id.EndsWith("A") ? 7.7f : 9.2f;
                var height = id.EndsWith("A") ? 1.95f : 2.25f;
                ProbeLine(id, new Vector3(0, height, length * .55f),
                    new Vector3(0, height, -length * .55f), "split end passage");
                ProbeLine(id, new Vector3(height * 1.2f, height, 0),
                    new Vector3(0, height, 0), "split side passage");
            }
            foreach (var id in new[] { "FAF-WOOD-009A", "FAF-WOOD-009B" })
            {
                ProbeLateral(id, 3.4f, 6f, -4.5f, -.3f);
                ProbeLine(id, new Vector3(-1.8f, 1.4f, 2),
                    new Vector3(-1.8f, 4.2f, .5f), "crown vertical");
            }
            var treeAnchor = plan.Single(p => p.Id == "FAF-WOOD-005A");
            var bed = PikeCoverFixture.Bed(treeAnchor.Position.X, treeAnchor.Position.Z);
            var fishStart = fish.GlobalTransform;
            fish.GlobalPosition = treeAnchor.Position + new Basis(Vector3.Up, treeAnchor.Yaw) *
                new Vector3(-3f, 1.15f, 0f);
            fish.GlobalBasis = Basis.Identity;
            Require(fish.TestMove(fish.GlobalTransform, new Basis(Vector3.Up, treeAnchor.Yaw) *
                new Vector3(6f, 0f, 0f)), "juvenile Bass passed through long tree trunk");
            fish.GlobalTransform = fishStart;
            Require(SolidOnSightline(scene,
                new Vector3(treeAnchor.Position.X - 6f, bed + 1f, treeAnchor.Position.Z),
                new Vector3(treeAnchor.Position.X + 6f, bed + 1f, treeAnchor.Position.Z),
                fish, pike), "long tree did not break line of sight");
            Require(!SolidOnSightline(scene, new Vector3(0, 1.2f, 18),
                new Vector3(0, -1.2f, -2), fish, pike),
                "exposed lane has unexpected hard cover");
            var tris = plan.Sum(p => catalog.Assets.Single(a => a.Id == p.Id).Triangles);
            var mats = plan.Sum(p => catalog.Assets.Single(a => a.Id == p.Id).Materials);
            Require(tris < 60000 && mats < 85 && actualShapes < 270,
                "fixture cover geometry budget");
            fish.SetInputMode("keyboard");
            var start = fish.GlobalPosition;
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
            var minCamera = float.PositiveInfinity;
            var cameraHits = 0;
            for (var frame = 0; frame < 520; frame++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                if (frame % 12 != 0) continue;
                var eye = camera.GlobalPosition;
                minCamera = Mathf.Min(minCamera, eye.DistanceTo(fish.GlobalPosition));
                if (Blocked(scene, cameraSphere, eye, fish, pike)) cameraHits++;
            }
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
            fish.ClearAndDisarmInput();
            var end = fish.GlobalPosition;
            Require(end.Z < 8f && start.DistanceTo(end) > 14f && cameraHits == 0 &&
                minCamera > .65f,
                $"real fish/camera crossing failed start={start} end={end} camera_hits={cameraHits} min_camera={minCamera:F2}");
            var hollow = plan.Single(p => p.Id == "FAF-WOOD-008A");
            fish.GlobalPosition = hollow.Position + new Basis(Vector3.Up, hollow.Yaw) *
                new Vector3(0, 1.95f, 7.7f * .55f + 1.4f);
            fish.ClearAndDisarmInput();
            for (var i = 0; i < 3; i++) await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            var hollowStart = fish.GlobalPosition;
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Pressed = true });
            var hollowHits = 0;
            for (var frame = 0; frame < 400; frame++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                if (frame % 12 == 0 && Blocked(scene, cameraSphere, camera.GlobalPosition, fish, pike))
                {
                    hollowHits++;
                    GD.Print($"PIKE_COVER_CAMERA_OVERLAP frame={frame} eye={camera.GlobalPosition} fish={fish.GlobalPosition} blocker={Blocker(scene, cameraSphere, camera.GlobalPosition, fish, pike)}");
                }
                if (fish.GlobalPosition.Z < hollow.Position.Z - 4.6f) break;
            }
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.W, Pressed = false });
            fish.ClearAndDisarmInput();
            var hollowEnd = fish.GlobalPosition;
            Require(hollowEnd.Z < hollow.Position.Z - 4f && hollowHits == 0,
                $"real split-hollow traverse failed start={hollowStart} end={hollowEnd} camera_hits={hollowHits}");
            fish.GlobalPosition = start;
            pike.GlobalPosition = new Vector3(0, PikeCoverFixture.Bed(0, 5) + 1.7f, 5);
            var pikeStart = pike.GlobalPosition;
            for (var i = 0; i < 84; i++)
            {
                var nextZ = pike.GlobalPosition.Z - .2f;
                var destination = new Vector3(0, PikeCoverFixture.Bed(0, nextZ) + 1.7f, nextZ);
                var collision = pike.MoveAndCollide(destination - pike.GlobalPosition);
                Require(collision is null, "real Pike actor blocked in search corridor");
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            Require(pike.GlobalPosition.Z < -11f,
                "real Pike actor did not reach deep approach");
            pike.GlobalPosition = hollow.Position + new Basis(Vector3.Up, hollow.Yaw) *
                new Vector3(0, 1.95f, 7.7f * .55f + 2.4f);
            for (var i = 0; i < 52; i++)
            {
                var collision = pike.MoveAndCollide(new Basis(Vector3.Up, hollow.Yaw) *
                    new Vector3(0, 0, -.2f));
                Require(collision is null, "triple-size Pike clipped during split hollow traverse");
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            }
            var pikeHollowLocal = new Basis(Vector3.Up, hollow.Yaw).Inverse() *
                (pike.GlobalPosition - hollow.Position);
            Require(pikeHollowLocal.Z < -3f,
                "triple-size Pike did not exit physically open hollow");
            // Reproduce a stationary turn under the authored bent tree branch.
            // Translation collision alone cannot keep a long body out of wood.
            var bent = plan.Single(p => p.Id == "FAF-WOOD-005B");
            var bentBasis = new Basis(Vector3.Up, bent.Yaw);
            var initialHeading = fish.GlobalBasis;
            var quarterTurn = new Basis(Vector3.Up, -Mathf.Pi * .5f) * initialHeading;
            var turnPose = Vector3.Inf;
            for (var x = 2.2f; x <= 3.8f && !turnPose.IsFinite(); x += .1f)
                for (var y = 1.4f; y <= 2.5f && !turnPose.IsFinite(); y += .1f)
                    for (var z = -.9f; z <= .9f; z += .1f)
                    {
                        var at = bent.Position + bentBasis * new Vector3(x, y, z);
                        if (BodyBlocked(at, initialHeading) || !BodyBlocked(at, quarterTurn))
                            continue;
                        turnPose = at;
                        break;
                    }
            Require(turnPose.IsFinite(), "no clear-to-blocked Bass turn pose by bent branch");
            fish.ClearAndDisarmInput();
            fish.GlobalPosition = turnPose;
            for (var i = 0; i < 3; i++) await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
            Require(!BodyBlocked(fish.GlobalPosition, fish.GlobalBasis),
                "Bass turn starts intersecting bent branch");
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.D, Pressed = true });
            var bodyHits = 0;
            for (var frame = 0; frame < 50; frame++)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.PhysicsFrame);
                if (BodyBlocked(fish.GlobalPosition, fish.GlobalBasis)) bodyHits++;
                var visual = fish.GetNodeOrNull<Node3D>("PlayerVisual");
                Require(visual is not null && visual.GlobalPosition.DistanceTo(fish.GlobalPosition) < .001f,
                    "Bass visual detached from authoritative body while turning");
            }
            Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = Key.D, Pressed = false });
            var turnDegrees = Mathf.RadToDeg(initialHeading.Z.AngleTo(fish.GlobalBasis.Z));
            Require(fish.GetLastYaw() > 0f && bodyHits == 0 && turnDegrees < 90f,
                $"Bass rotated through bent branch input={fish.GetLastYaw():F2} turn={turnDegrees:F1} overlaps={bodyHits}");
            GD.Print($"PIKE_BASS_BRANCH_TURN_OK pose={turnPose} turn_degrees={turnDegrees:F1} overlaps={bodyHits}");
            GD.Print($"PIKE_COVER_CHECK_OK seed={PikeCoverFixture.Seed} variants=23 families=10 instances={plan.Count} triangles={tris} materials={mats} shapes={actualShapes} zones=3 wood_support_max={supportMax:F2} fish_end={end} hollow_end={hollowEnd} pike_hollow_end={pike.GlobalPosition} camera_hits={cameraHits + hollowHits}");
            tree.Quit();

            bool BodyBlocked(Vector3 at, Basis heading)
            {
                var body = fish.GetNode<CollisionShape3D>("Collision");
                using var query = new PhysicsShapeQueryParameters3D
                {
                    Shape = body.Shape,
                    Transform = new Transform3D(heading, at) * body.Transform,
                    CollisionMask = fish.CollisionMask,
                    Exclude = new Godot.Collections.Array<Rid> { fish.GetRid(), pike.GetRid() },
                };
                return scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count > 0;
            }

            void ProbeLine(string id, Vector3 start, Vector3 end, string role)
            {
                var placement = plan.Single(p => p.Id == id);
                var basis = new Basis(Vector3.Up, placement.Yaw);
                for (var i = 0; i <= 12; i++)
                {
                    var local = start.Lerp(end, i / 12f);
                    var at = placement.Position + basis * local;
                    Require(!Blocked(scene, fishSphere, at, fish, pike), id + " " + role + " fish gap " + i + " at=" + at + " collider=" + Blocker(scene, fishSphere, at, fish, pike));
                    Require(!Blocked(scene, cameraSphere, at, fish, pike), id + " " + role + " camera gap " + i);
                }
            }

            void ProbeLateral(string id, float height, float halfWidth, float zMin, float zMax)
            {
                var placement = plan.Single(p => p.Id == id);
                var basis = new Basis(Vector3.Up, placement.Yaw);
                var nx = (int)MathF.Round(2 * halfWidth / .4f) + 1;
                var nz = (int)MathF.Round((zMax - zMin) / .3f) + 1;
                var clear = new bool[nx, nz];
                for (var ix = 0; ix < nx; ix++)
                    for (var iz = 0; iz < nz; iz++)
                    {
                        var local = new Vector3(-halfWidth + ix * .4f, height, zMin + iz * .3f);
                        var at = placement.Position + basis * local;
                        clear[ix, iz] = !Blocked(scene, fishSphere, at, fish, pike) &&
                            !Blocked(scene, cameraSphere, at, fish, pike);
                    }
                var queue = new Queue<(int x, int z)>();
                var seen = new bool[nx, nz];
                for (var iz = 0; iz < nz; iz++)
                    if (clear[0, iz]) { queue.Enqueue((0, iz)); seen[0, iz] = true; }
                var reached = false;
                while (queue.Count > 0)
                {
                    var (x, z) = queue.Dequeue();
                    if (x == nx - 1) { reached = true; break; }
                    foreach (var (dx, dz) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        var xx = x + dx; var zz = z + dz;
                        if (xx < 0 || xx >= nx || zz < 0 || zz >= nz ||
                            seen[xx, zz] || !clear[xx, zz]) continue;
                        seen[xx, zz] = true;
                        queue.Enqueue((xx, zz));
                    }
                }
                if (!reached)
                    for (var iz = nz - 1; iz >= 0; iz--)
                        GD.Print(id + " lateral-grid z=" + iz + " " +
                            new string(Enumerable.Range(0, nx).Select(ix => clear[ix, iz] ? '.' : '#').ToArray()));
                Require(reached, id + " has no fish/camera-sized lateral passage");
            }
        }
        catch (Exception error)
        {
            GD.PushError("PIKE_COVER_CHECK_FAILED " + error);
            tree.Quit(1);
        }
    }

    private static bool Blocked(PikeEncounter scene, SphereShape3D sphere, Vector3 at,
        FishController fish, CharacterBody3D pike)
    {
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = sphere, Transform = new Transform3D(Basis.Identity, at),
            CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { fish.GetRid(), pike.GetRid() },
        };
        return scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count > 0;
    }

    private static string Blocker(PikeEncounter scene, SphereShape3D sphere, Vector3 at,
        FishController fish, CharacterBody3D pike)
    {
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = sphere, Transform = new Transform3D(Basis.Identity, at),
            CollisionMask = 1,
            Exclude = new Godot.Collections.Array<Rid> { fish.GetRid(), pike.GetRid() },
        };
        var hits = scene.GetWorld3D().DirectSpaceState.IntersectShape(query, 1);
        return hits.Count == 0 ? "clear" : hits[0]["collider"].ToString();
    }

    private static bool SolidOnSightline(PikeEncounter scene, Vector3 from, Vector3 to,
        FishController fish, CharacterBody3D pike)
    {
        var probe = new SphereShape3D { Radius = .035f };
        var count = (int)MathF.Ceiling(from.DistanceTo(to) / .1f);
        for (var i = 1; i < count; i++)
            if (Blocked(scene, probe, from.Lerp(to, i / (float)count), fish, pike))
                return true;
        return false;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static bool HasCollision(Node node) =>
        node is CollisionObject3D or CollisionShape3D || node.GetChildren().Any(HasCollision);
}
