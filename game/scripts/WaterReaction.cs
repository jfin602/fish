#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Read-only mapping from the accepted motor's presentation values to emitter ratios.</summary>
public readonly record struct WaterReactionLevels(float Left, float Right, float Bubbles)
{
    public static WaterReactionLevels FromMovement(float speed, float effort, float acceleration, float turn)
    {
        speed = Mathf.Clamp(float.IsFinite(speed) ? speed : 0, 0, 1);
        effort = Mathf.Clamp(float.IsFinite(effort) ? effort : 0, 0, 1);
        acceleration = Mathf.Clamp(float.IsFinite(acceleration) ? acceleration : 0, 0, 1);
        turn = Mathf.Clamp(float.IsFinite(turn) ? turn : 0, -1, 1);
        var wake = speed * (0.05f + 0.26f * effort + 0.28f * acceleration);
        var outside = speed * 0.36f * Mathf.Abs(turn);
        return new WaterReactionLevels(
            Mathf.Clamp(wake + (turn > 0 ? outside : 0), 0, 1),
            Mathf.Clamp(wake + (turn < 0 ? outside : 0), 0, 1),
            Mathf.Clamp(speed * (0.02f + 0.14f * effort + 0.24f * acceleration +
                0.10f * Mathf.Abs(turn)), 0, 1));
    }
}

/// <summary>World-space GPU wake below the player root; never drives the fish body.</summary>
public partial class WaterReaction : Node3D
{
    public const int PondRingCapacity = 8;
    private readonly FishController _fish;
    private readonly WaterVisualProfile? _surfaceProfile;
    private readonly float _pondWaterY;
    private readonly MeshInstance3D[] _rings = new MeshInstance3D[PondRingCapacity];
    private readonly float[] _ringAges = new float[PondRingCapacity];
    private GpuParticles3D? _splash;
    private int _ringCursor;
    private float _surfaceClock;
    private float _splashCooldown;
    private GpuParticles3D _left = null!;
    private GpuParticles3D _right = null!;
    private GpuParticles3D _bubbles = null!;
    private bool _enabled = true;
    private bool _suspended = true;
    private float _lastSpeed;

    public WaterReaction(FishController fish, WaterVisualProfile? surfaceProfile = null, float pondWaterY = 0f)
    {
        _fish = fish;
        _surfaceProfile = surfaceProfile?.Id == WorldEnvironmentProfile.Pond ? surfaceProfile : null;
        _pondWaterY = pondWaterY;
        Name = "WaterReaction";
    }

    public override void _Ready()
    {
        // Fish3 is 0.12 scale and turned 180 degrees under this unscaled fish root.
        // Its tail is near local +Z. The emitters inherit fish pitch/yaw, while
        // emitted particles keep their world positions instead of following camera/fish.
        _left = MakeWake("LeftWake", -0.20f, -1);
        _right = MakeWake("RightWake", 0.20f, 1);
        _bubbles = MakeBubbles();
        if (_surfaceProfile is not null) MakePondSurfaceEffects();
        Clear();
        _lastSpeed = _fish.PresentationSpeed;
    }

    private GpuParticles3D MakeWake(string name, float x, int side)
    {
        var material = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.54f, 0.85f, 0.88f, 0.30f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            VertexColorUseAsAlbedo = true,
            UseParticleTrails = true,
        };
        var ribbon = new RibbonTrailMesh
        {
            Size = 0.06f,
            Sections = 3,
            SectionLength = 0.12f,
            SectionSegments = 1,
            Material = material,
        };
        var process = new ParticleProcessMaterial
        {
            Direction = new Vector3(side * 0.35f, 0.02f, 1).Normalized(),
            Spread = 20,
            Gravity = Vector3.Zero,
            InitialVelocityMin = 0.20f,
            InitialVelocityMax = 0.42f,
            ScaleMin = 0.65f,
            ScaleMax = 1.0f,
            Color = Colors.White,
        };
        var particles = new GpuParticles3D
        {
            Name = name,
            Position = new Vector3(x, -0.04f, 0.43f),
            Amount = 16,
            AmountRatio = 0,
            Lifetime = 0.55,
            Preprocess = 0,
            Explosiveness = 0,
            LocalCoords = false,
            TrailEnabled = true,
            TrailLifetime = 0.22,
            VisibilityAabb = new Aabb(new Vector3(-1.5f, -1.2f, -0.4f), new Vector3(3, 2.4f, 2.8f)),
            ProcessMaterial = process,
            DrawPass1 = ribbon,
            Emitting = false,
        };
        AddChild(particles);
        return particles;
    }

    private GpuParticles3D MakeBubbles()
    {
        var material = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.75f, 0.95f, 0.97f, 0.34f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
            VertexColorUseAsAlbedo = true,
        };
        var process = new ParticleProcessMaterial
        {
            Direction = new Vector3(0, 0.75f, 0.25f).Normalized(),
            Spread = 25,
            Gravity = Vector3.Zero,
            InitialVelocityMin = 0.10f,
            InitialVelocityMax = 0.28f,
            ScaleMin = 0.6f,
            ScaleMax = 1,
            Color = Colors.White,
        };
        var particles = new GpuParticles3D
        {
            Name = "TailBubbles",
            Position = new Vector3(0, 0.08f, 0.43f),
            Amount = 12,
            AmountRatio = 0,
            Lifetime = 0.42,
            Preprocess = 0,
            Explosiveness = 0,
            LocalCoords = false,
            VisibilityAabb = new Aabb(new Vector3(-1, -1, -0.3f), new Vector3(2, 2, 2)),
            ProcessMaterial = process,
            DrawPass1 = new SphereMesh { Radius = 0.025f, Height = 0.05f, Material = material },
            Emitting = false,
        };
        AddChild(particles);
        return particles;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_enabled || GetTree().Paused || !_fish.GetInputArmed() || delta <= 0)
        {
            if (!_suspended) Suspend();
            return;
        }
        if (_suspended)
        {
            _lastSpeed = _fish.PresentationSpeed;
            _suspended = false;
        }
        var speed = _fish.PresentationSpeed;
        var tuning = _fish.Tuning;
        var acceleration = Mathf.Max(0, (speed - _lastSpeed) / (float)delta) /
            Mathf.Max(0.001f, tuning.Acceleration);
        _lastSpeed = speed;
        var levels = WaterReactionLevels.FromMovement(
            speed / Mathf.Max(0.001f, tuning.MaxSpeed),
            _fish.PresentationEffort, acceleration, _fish.PresentationTurn);
        Apply(_left, levels.Left);
        Apply(_right, levels.Right);
        Apply(_bubbles, levels.Bubbles);
        if (_surfaceProfile is not null)
            SamplePondSurface(_fish.GlobalPosition, _fish.Velocity, _fish.PresentationEffort, (float)delta);
    }

    public int ActivePondRings
    {
        get
        {
            var count = 0;
            foreach (var ring in _rings) if (ring is { Visible: true }) count++;
            return count;
        }
    }

    public static float SurfaceContactLevel(float speed01, float effort, float depth)
    {
        if (!float.IsFinite(speed01) || !float.IsFinite(effort) || !float.IsFinite(depth)) return 0f;
        var proximity = Mathf.SmoothStep(0f, 1f, Mathf.Clamp((1.05f - depth) / .9f, 0f, 1f));
        return proximity * Mathf.Clamp(speed01, 0f, 1f) * (.18f + .82f * Mathf.Clamp(effort, 0f, 1f));
    }

    /// <summary>Presentation-only sampling of the motor's world pose and velocity.</summary>
    public void SamplePondSurface(Vector3 position, Vector3 velocity, float effort, float delta)
    {
        if (_surfaceProfile is null || delta <= 0f) return;
        for (var i = 0; i < PondRingCapacity; i++)
        {
            if (!_rings[i].Visible) continue;
            _ringAges[i] += delta;
            if (_ringAges[i] >= .95f) { _rings[i].Visible = false; continue; }
            var progress = _ringAges[i] / .95f;
            var scale = .22f + progress * (1.0f + .55f * _surfaceProfile.SurfaceWakeStrength);
            _rings[i].Scale = new Vector3(scale, 1f, scale * 1.42f);
            var material = (StandardMaterial3D)_rings[i].MaterialOverride;
            material.AlbedoColor = new Color(.61f, .67f, .53f, .25f * (1f - progress));
        }
        _splashCooldown = Mathf.Max(0f, _splashCooldown - delta);
        var horizontal = new Vector3(velocity.X, 0, velocity.Z);
        var speed01 = horizontal.Length() / Mathf.Max(.001f, _fish.Tuning.MaxSpeed);
        var depth = _pondWaterY - position.Y;
        var level = SurfaceContactLevel(speed01, effort, depth);
        if (level < .025f || depth < -.1f || depth > 1.1f)
        {
            _surfaceClock = 0f;
            return;
        }
        _surfaceClock += delta;
        var interval = _surfaceProfile.SurfaceRippleInterval / (0.65f + level);
        if (_surfaceClock < interval) return;
        _surfaceClock = 0f;
        var ring = _rings[_ringCursor];
        _ringCursor = (_ringCursor + 1) % PondRingCapacity;
        ring.GlobalPosition = new Vector3(position.X, _pondWaterY + .045f, position.Z);
        ring.GlobalRotation = new Vector3(0, Mathf.Atan2(horizontal.X, horizontal.Z), 0);
        ring.Scale = new Vector3(.22f, 1f, .31f);
        _ringAges[(_ringCursor + PondRingCapacity - 1) % PondRingCapacity] = 0f;
        ring.Visible = true;
        if (_splash is not null && depth < .32f && level > .42f && effort > .4f &&
            _splashCooldown <= 0f)
        {
            _splash.GlobalPosition = ring.GlobalPosition;
            _splash.Restart();
            _splash.Emitting = true;
            _splashCooldown = .7f;
        }
    }

    private void MakePondSurfaceEffects()
    {
        using var tool = new SurfaceTool();
        tool.Begin(Mesh.PrimitiveType.Triangles);
        const int segments = 20;
        for (var i = 0; i < segments; i++)
        {
            var a = i * Mathf.Tau / segments;
            var b = (i + 1) * Mathf.Tau / segments;
            var innerA = new Vector3(Mathf.Cos(a) * .87f, 0, Mathf.Sin(a) * .87f);
            var outerA = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
            var innerB = new Vector3(Mathf.Cos(b) * .87f, 0, Mathf.Sin(b) * .87f);
            var outerB = new Vector3(Mathf.Cos(b), 0, Mathf.Sin(b));
            foreach (var point in new[] { innerA, outerB, outerA, innerA, innerB, outerB })
            { tool.SetNormal(Vector3.Up); tool.AddVertex(point); }
        }
        var mesh = tool.Commit();
        for (var i = 0; i < PondRingCapacity; i++)
        {
            var material = new StandardMaterial3D
            {
                AlbedoColor = new Color(.61f, .67f, .53f, 0f),
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            };
            var ring = new MeshInstance3D { Name = $"SurfaceRing{i}", Mesh = mesh,
                MaterialOverride = material, TopLevel = true, Visible = false,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
            AddChild(ring);
            _rings[i] = ring;
        }
        _splash = new GpuParticles3D
        {
            Name = "SurfaceSplash", TopLevel = true, Amount = 6, Lifetime = .28,
            OneShot = true, Emitting = false, LocalCoords = false,
            VisibilityAabb = new Aabb(new Vector3(-.5f, -.3f, -.5f), Vector3.One),
            ProcessMaterial = new ParticleProcessMaterial
            {
                Direction = Vector3.Up, Spread = 33f, Gravity = new Vector3(0, -2.1f, 0),
                InitialVelocityMin = .3f, InitialVelocityMax = .8f,
                ScaleMin = .5f, ScaleMax = 1f,
            },
            DrawPass1 = new SphereMesh { Radius = .018f, Height = .036f,
                Material = new StandardMaterial3D { AlbedoColor = new Color(.68f, .72f, .60f, .55f),
                    Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                    ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded } },
        };
        AddChild(_splash);
    }

    private static void Apply(GpuParticles3D emitter, float ratio)
    {
        emitter.AmountRatio = ratio;
        emitter.Emitting = ratio > 0.001f;
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        Suspend();
    }

    // Pure mapping preview also gives P7 an exact tuning readout.
    public Vector3 PreviewLevels(float speed, float effort, float acceleration, float turn)
    {
        var levels = WaterReactionLevels.FromMovement(speed, effort, acceleration, turn);
        return new Vector3(levels.Left, levels.Right, levels.Bubbles);
    }

    public void Suspend()
    {
        if (!IsNodeReady()) return;
        Clear();
        for (var i = 0; i < PondRingCapacity; i++)
            if (_rings[i] is not null) { _rings[i].Visible = false; _ringAges[i] = 0f; }
        _surfaceClock = 0f;
        _splashCooldown = 0f;
        if (_splash is not null) { _splash.Restart(); _splash.Emitting = false; }
        _lastSpeed = _fish.PresentationSpeed;
        _suspended = true;
    }

    private void Clear()
    {
        foreach (var emitter in new[] { _left, _right, _bubbles })
        {
            emitter.AmountRatio = 0;
            emitter.Restart();
            // Restart clears prior GPU history but also sets emitting=true in 4.7.
            emitter.Emitting = false;
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut || what == NotificationWMWindowFocusOut)
            Suspend();
    }
}
