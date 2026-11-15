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

/// <summary>World-space wake and surface effects; never drives the fish body.</summary>
public partial class WaterReaction : Node3D
{
    public const int PondRingCapacity = 8;
    private readonly FishController _fish;
    private readonly WaterVisualProfile _waterProfile;
    private readonly WaterVisualProfile? _surfaceProfile;
    private readonly MeshInstance3D[] _rings = new MeshInstance3D[PondRingCapacity];
    private readonly float[] _ringAges = new float[PondRingCapacity];
    private readonly float[] _ringSizes = new float[PondRingCapacity];
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
    private float _boostBlend;
    private int _boostVisualStep = -1;
    private float _bodyScale = 1f;
    private float _bodyRadius = .48f;
    private float _visualTopReach = .2f;
    private float _tailZ = .43f;
    private float _trailTimeScale = 1f;
    private PlayerBodyGeometry _contactBody;
    private readonly PondWakePath _wakePath = new();
    private PondWakeContacts? _latestContact;
    private float _latestContactEnergy;
    private ImmediateMesh? _ribbonMesh;
    private MeshInstance3D? _ribbonView;
    private ShaderMaterial? _ribbonMaterial;
    private UnderwaterAtmosphere? _pondAtmosphere;
    private SeabedSurface? _pondSeabed;
    private readonly bool _legacyRings = OS.IsDebugBuild() &&
        Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--wake-legacy-rings");
    private bool _showLegacyRings;
    private readonly bool _lowSurfaceEffects = Array.Exists(OS.GetCmdlineUserArgs(),
        arg => arg == "--effects=low");
    public int ActiveWakeTriangles { get; private set; }
    private int _stageRevision;
    private ImmediateMesh? _wakeDebugMesh;
    private MeshInstance3D? _wakeDebugView;
    public PondWakePath WakePath => _wakePath;

    public WaterReaction(FishController fish, WaterVisualProfile? surfaceProfile = null)
    {
        _fish = fish;
        _waterProfile = surfaceProfile ?? WaterVisualProfile.Ocean;
        _surfaceProfile = surfaceProfile?.Id == WorldEnvironmentProfile.Pond ? surfaceProfile : null;
        Name = "WaterReaction";
    }

    public override void _Ready()
    {
        _showLegacyRings = _legacyRings;
        _left = MakeWake("LeftWake", -0.20f, -1);
        _right = MakeWake("RightWake", 0.20f, 1);
        _bubbles = MakeBubbles();
        if (_surfaceProfile is not null)
        {
            _pondAtmosphere = _fish.GetParent()?.GetNodeOrNull<UnderwaterAtmosphere>("UnderwaterAtmosphere");
            _pondSeabed = _fish.GetParent()?.GetNodeOrNull<SeabedSurface>("Habitat/Seabed");
            MakePondSurfaceEffects();
            if (Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--effects=off"))
                _enabled = false;
        }
        #if DEBUG
        if (_surfaceProfile is not null &&
            (Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--wake-path-debug") ||
             OS.GetEnvironment("FISH_WAKE_PATH_DEBUG") == "1"))
            MakeWakeDebugView();
        #endif
        Clear();
        if (_fish.GetNodeOrNull<Node3D>("PlayerVisual") is not null)
            ConfigureFromPlayerVisual();
        _lastSpeed = _fish.PresentationSpeed;
    }

    /// <summary>Fit the same wake to the current player mesh in any world or life stage.</summary>
    public void ConfigureFromPlayerVisual()
    {
        var body = PlayerBodyGeometry.Measure(_fish);
        _contactBody = body;
        _stageRevision++;
        _wakePath.Clear();
        _bodyRadius = body.Radius;
        _visualTopReach = body.TopReach;
        _bodyScale = Mathf.Clamp(body.Length / .95f, .025f, 4f);
        _trailTimeScale = Mathf.Clamp(Mathf.Sqrt(_bodyScale), .55f, 1.2f);
        _boostVisualStep = -1;
        _tailZ = body.TailZ + body.Radius * .08f;
        if (!IsNodeReady()) return;
        var side = body.Radius * .42f;
        var wakeCenterZ = _tailZ - body.Length * .22f;
        _left.Position = new Vector3(-side, -body.Radius * .08f, wakeCenterZ);
        _right.Position = new Vector3(side, -body.Radius * .08f, wakeCenterZ);
        _bubbles.Position = new Vector3(0f, body.Radius * .15f,
            _tailZ - body.Length * .10f);
        foreach (var wake in new[] { _left, _right })
        {
            var puff = (SphereMesh)wake.DrawPass1;
            puff.Radius = .012f * _bodyScale;
            puff.Height = .042f * _bodyScale;
            wake.Lifetime = .85f * _trailTimeScale;
            var process = (ParticleProcessMaterial)wake.ProcessMaterial;
            process.InitialVelocityMin = .20f * _bodyScale;
            process.InitialVelocityMax = .42f * _bodyScale;
            process.EmissionBoxExtents = new Vector3(body.Radius * .28f,
                body.Radius * .12f, body.Length * .25f);
            wake.VisibilityAabb = new Aabb(new Vector3(-2f, -2f, -1f) * _bodyScale,
                new Vector3(4f, 4f, 4f) * _bodyScale);
        }
        var bubble = (SphereMesh)_bubbles.DrawPass1;
        bubble.Radius = .018f * _bodyScale;
        bubble.Height = .036f * _bodyScale;
        _bubbles.Lifetime = .60f * _trailTimeScale;
        var bubbleProcess = (ParticleProcessMaterial)_bubbles.ProcessMaterial;
        bubbleProcess.InitialVelocityMin = .10f * _bodyScale;
        bubbleProcess.InitialVelocityMax = .28f * _bodyScale;
        bubbleProcess.EmissionBoxExtents = new Vector3(body.Radius * .3f,
            body.Radius * .15f, body.Length * .15f);
        if (_splash is not null)
        {
            var drop = (SphereMesh)_splash.DrawPass1;
            drop.Radius = .014f * _bodyScale;
            drop.Height = .028f * _bodyScale;
            var splashProcess = (ParticleProcessMaterial)_splash.ProcessMaterial;
            splashProcess.InitialVelocityMin = .3f * _bodyScale;
            splashProcess.InitialVelocityMax = .8f * _bodyScale;
        }
        Suspend();
    }

    private GpuParticles3D MakeWake(string name, float x, int side)
    {
        var wakeColor = _waterProfile.Id == WorldEnvironmentProfile.Pond
            ? _waterProfile.SkyHorizon.Lerp(_waterProfile.UnderwaterSurfaceScatter, .5f)
            : _waterProfile.SurfaceLight.Lerp(_waterProfile.Volume, .5f);
        var material = new StandardMaterial3D
        {
            AlbedoColor = new Color(wakeColor.R, wakeColor.G, wakeColor.B, .21f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            VertexColorUseAsAlbedo = true,
        };
        // Short individual puffs cannot stretch into a screen-spanning GPU ribbon
        // when the fish changes scale, turns quickly, or enters from a hatch.
        var puff = new SphereMesh
        {
            Radius = .012f,
            Height = .042f,
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
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(.08f, .03f, .2f),
            LifetimeRandomness = .55f,
        };
        var particles = new GpuParticles3D
        {
            Name = name,
            Position = new Vector3(x, -0.04f, 0.43f),
            Amount = 16,
            AmountRatio = 0,
            Lifetime = 0.85,
            Preprocess = 0,
            Explosiveness = 0,
            LocalCoords = false,
            VisibilityAabb = new Aabb(new Vector3(-1.5f, -1.2f, -0.4f), new Vector3(3, 2.4f, 2.8f)),
            ProcessMaterial = process,
            DrawPass1 = puff,
            Emitting = false,
        };
        AddChild(particles);
        return particles;
    }

    private GpuParticles3D MakeBubbles()
    {
        var bubbleColor = _waterProfile.Id == WorldEnvironmentProfile.Pond
            ? _waterProfile.SkyHorizon.Lerp(_waterProfile.UnderwaterSurfaceScatter, .35f)
            : _waterProfile.SurfaceLight.Lerp(_waterProfile.Volume, .65f);
        var material = new StandardMaterial3D
        {
            AlbedoColor = new Color(bubbleColor.R, bubbleColor.G, bubbleColor.B, .28f),
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
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(.08f, .04f, .12f),
            LifetimeRandomness = .45f,
        };
        var particles = new GpuParticles3D
        {
            Name = "TailBubbles",
            Position = new Vector3(0, 0.08f, 0.43f),
            Amount = 12,
            AmountRatio = 0,
            Lifetime = 0.60,
            Preprocess = 0,
            Explosiveness = 0,
            LocalCoords = false,
            VisibilityAabb = new Aabb(new Vector3(-1, -1, -0.3f), new Vector3(2, 2, 2)),
            ProcessMaterial = process,
            DrawPass1 = new SphereMesh { Radius = 0.018f, Height = 0.036f, Material = material },
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
        var actualSpeed = _fish.Velocity.Length();
        var speed01 = actualSpeed / Mathf.Max(.001f, tuning.MaxSpeed);
        var levels = WaterReactionLevels.FromMovement(
            speed01,
            _fish.PresentationEffort, acceleration, _fish.PresentationTurn);
        var boostTarget = _fish.BoostActive && _fish.BoostEnergy > 0f
            ? Mathf.Clamp(speed01, 0f, 1f) * Mathf.Clamp(_fish.PresentationEffort, 0f, 1f) : 0f;
        _boostBlend = Mathf.Lerp(_boostBlend, boostTarget, 1f - Mathf.Exp(-5f * (float)delta));
        var rateScale = _trailTimeScale;
        Apply(_left, Mathf.Clamp(levels.Left * rateScale * (1f + .55f * _boostBlend), 0f, 1f));
        Apply(_right, Mathf.Clamp(levels.Right * rateScale * (1f + .55f * _boostBlend), 0f, 1f));
        Apply(_bubbles, Mathf.Clamp(levels.Bubbles * rateScale * (1f + .4f * _boostBlend), 0f, 1f));
        var visualStep = Mathf.RoundToInt(_boostBlend * 6f);
        if (visualStep != _boostVisualStep)
        {
            _boostVisualStep = visualStep;
            var visualBoost = visualStep / 6f;
            _left.Lifetime = _right.Lifetime = .85f * _trailTimeScale *
                (1f + .28f * visualBoost);
            var width = .012f * _bodyScale * (1f + .18f * visualBoost);
            ((SphereMesh)_left.DrawPass1).Radius = width;
            ((SphereMesh)_right.DrawPass1).Radius = width;
        }
        if (_surfaceProfile is not null)
        {
            var contact = PondWakeContacts.At(_contactBody, _fish.GlobalTransform,
                _fish.PondSurfaceHeightAt);
            var horizontalSpeed = new Vector2(_fish.Velocity.X, _fish.Velocity.Z).Length();
            var energy = horizontalSpeed / Mathf.Max(.001f, _fish.Tuning.MaxSpeed) *
                (.2f + .8f * Mathf.Clamp(_fish.PresentationEffort, 0f, 1f)) *
                (1f + .35f * _boostBlend);
            _wakePath.Step(contact, _contactBody.Length, energy, _stageRevision, (float)delta);
            _latestContact = contact.Touching && energy > .03f ? contact : null;
            _latestContactEnergy = energy;
            DrawWakeDebug(contact);
            if (_showLegacyRings)
                SamplePondSurface(_fish.GlobalPosition, _fish.Velocity, _fish.PresentationEffort, (float)delta);
            else
                SampleBreach(contact, (float)delta);
        }
    }

    public override void _Process(double delta)
    {
        if (_surfaceProfile is null || _ribbonMesh is null || _ribbonMaterial is null ||
            _pondAtmosphere is null || _showLegacyRings) return;
        if (!_enabled || _suspended || GetTree().Paused)
        {
            _ribbonMesh.ClearSurfaces();
            ActiveWakeTriangles = 0;
            return;
        }
        _ribbonMaterial.SetShaderParameter("surface_illumination", _pondAtmosphere.SurfaceWakeIllumination);
        _ribbonMaterial.SetShaderParameter("night_weight", _pondAtmosphere.SurfaceWakeNightWeight);
        _ribbonMaterial.SetShaderParameter("sun_direction", _pondAtmosphere.SurfaceWakeSunDirection);
        var sun = _pondAtmosphere.SurfaceWakeSunColor;
        _ribbonMaterial.SetShaderParameter("sun_color", new Vector3(sun.R, sun.G, sun.B));
        ActiveWakeTriangles = PondWakeRibbon.Draw(_ribbonMesh, _wakePath, _wakePath.Seconds,
            _pondAtmosphere.GetRenderedSurfaceHeight, ShoreDistance, _ribbonMaterial,
            _lowSurfaceEffects ? .5f : 1f, _latestContact, _latestContactEnergy);
    }

    private float ShoreDistance(float worldX, float worldZ)
    {
        if (_pondSeabed is null) return float.MaxValue;
        var point = _pondSeabed.ToLocal(new Vector3(worldX, 0f, worldZ));
        return _pondSeabed.Basin.DistanceToShore(point.X, point.Z);
    }

    private void SampleBreach(PondWakeContacts contact, float delta)
    {
        _splashCooldown = Mathf.Max(0f, _splashCooldown - delta);
        if (_splash is null || _splashCooldown > 0f || !contact.Touching ||
            _wakePath.Count < 2 || _fish.Velocity.Y < .12f ||
            _fish.PresentationEffort < .45f || _lowSurfaceEffects) return;
        // Only a rising body crossing its interface makes droplets. Cruise/boost
        // at a fixed depth leaves the paired surface fronts without splashes.
        var y = _fish.GlobalPosition.Y + _visualTopReach * .35f;
        if (Mathf.Abs(y - contact.SurfaceY) > _bodyRadius * .3f) return;
        _splash.GlobalPosition = contact.Bow + Vector3.Up * .02f;
        _splash.Restart();
        _splash.Emitting = true;
        _splashCooldown = 1.25f;
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

    public static float SurfaceContactLevel(float speed01, float effort, float depth, float radius)
    {
        if (!float.IsFinite(radius) || radius <= 0f) return 0f;
        var reach = radius * 1.5f;
        var proximity = Mathf.SmoothStep(0f, 1f,
            Mathf.Clamp((reach - depth) / Mathf.Max(.002f, radius * 1.2f), 0f, 1f));
        return proximity * Mathf.Clamp(speed01, 0f, 1f) *
            (.18f + .82f * Mathf.Clamp(effort, 0f, 1f));
    }

    /// <summary>Presentation-only sampling of the motor's world pose and velocity.</summary>
    public void SamplePondSurface(Vector3 position, Vector3 velocity, float effort, float delta)
    {
        if (_surfaceProfile is null || !_showLegacyRings || delta <= 0f) return;
        for (var i = 0; i < PondRingCapacity; i++)
        {
            if (!_rings[i].Visible) continue;
            var ringPosition = _rings[i].GlobalPosition;
            _rings[i].GlobalPosition = new Vector3(ringPosition.X,
                _fish.PondSurfaceHeightAt(ringPosition.X, ringPosition.Z) + .045f,
                ringPosition.Z);
            _ringAges[i] += delta;
            if (_ringAges[i] >= .95f) { _rings[i].Visible = false; continue; }
            var progress = _ringAges[i] / .95f;
            var scale = _ringSizes[i] * (.22f + progress * (1.0f + .55f * _surfaceProfile.SurfaceWakeStrength));
            _rings[i].Scale = new Vector3(scale, 1f, scale * 1.42f);
            var material = (StandardMaterial3D)_rings[i].MaterialOverride;
            var tint = _waterProfile.SurfaceDark.Lerp(_waterProfile.SurfaceLight, .25f);
            material.AlbedoColor = new Color(tint.R, tint.G, tint.B, .25f * (1f - progress));
        }
        _splashCooldown = Mathf.Max(0f, _splashCooldown - delta);
        var horizontal = new Vector3(velocity.X, 0, velocity.Z);
        var speed01 = horizontal.Length() / Mathf.Max(.001f, _fish.Tuning.MaxSpeed);
        var depth = _fish.PondSurfaceHeightAt(position.X, position.Z) - position.Y;
        var contactReach = _visualTopReach + Mathf.Max(.002f, _bodyRadius * .08f);
        var level = SurfaceContactLevel(speed01, effort, depth, contactReach);
        level = Mathf.Clamp(level * (1f + .45f * _boostBlend), 0f, 1f);
        if (level < .025f || depth < -_bodyRadius * .2f || depth > contactReach)
        {
            _surfaceClock = 0f;
            return;
        }
        _surfaceClock += delta;
        var interval = _surfaceProfile.SurfaceRippleInterval / (0.65f + level + .2f * _boostBlend);
        if (_surfaceClock < interval) return;
        _surfaceClock = 0f;
        var ring = _rings[_ringCursor];
        _ringCursor = (_ringCursor + 1) % PondRingCapacity;
        var trail = new Vector3(velocity.X, 0f, velocity.Z).Normalized();
        var ringX = position.X - trail.X * Mathf.Min(_bodyRadius, _bodyScale * .4f);
        var ringZ = position.Z - trail.Z * Mathf.Min(_bodyRadius, _bodyScale * .4f);
        ring.GlobalPosition = new Vector3(ringX,
            _fish.PondSurfaceHeightAt(ringX, ringZ) + .045f, ringZ);
        ring.GlobalRotation = new Vector3(0, Mathf.Atan2(horizontal.X, horizontal.Z), 0);
        var emitted = (_ringCursor + PondRingCapacity - 1) % PondRingCapacity;
        _ringSizes[emitted] = _bodyScale * (1f + .22f * _boostBlend);
        ring.Scale = new Vector3(.22f * _ringSizes[emitted], 1f, .31f * _ringSizes[emitted]);
        _ringAges[emitted] = 0f;
        ring.Visible = true;
        if (_splash is not null && depth <= _visualTopReach &&
            level > .42f && effort > .4f &&
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
        _ribbonMesh = new ImmediateMesh();
        _ribbonMaterial = new ShaderMaterial
        {
            Shader = ResourceLoader.Load<Shader>("res://shaders/pond_wake.gdshader"),
            // Both the water plane and wake are transparent. Draw the wake after
            // the plane from either camera side so the green underside survives.
            RenderPriority = 1,
        };
        _ribbonMaterial.SetShaderParameter("surface_dark", new Vector3(
            _waterProfile.SurfaceDark.R, _waterProfile.SurfaceDark.G, _waterProfile.SurfaceDark.B));
        _ribbonMaterial.SetShaderParameter("surface_light", new Vector3(
            _waterProfile.SurfaceLight.R, _waterProfile.SurfaceLight.G, _waterProfile.SurfaceLight.B));
        _ribbonView = new MeshInstance3D
        {
            Name = "PondWakeFronts", Mesh = _ribbonMesh, TopLevel = true,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            ExtraCullMargin = 4f,
        };
        AddChild(_ribbonView);
        if (_legacyRings) MakeLegacyRings();
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
            DrawPass1 = new SphereMesh { Radius = .014f, Height = .028f,
                Material = new StandardMaterial3D { AlbedoColor = new Color(
                    _waterProfile.SurfaceLight.R, _waterProfile.SurfaceLight.G,
                    _waterProfile.SurfaceLight.B, .24f),
                    Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                    ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded } },
        };
        AddChild(_splash);
    }

    /// <summary>Debug-only comparison mode for fixed-route native A/B captures.</summary>
    private void MakeLegacyRings()
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
                AlbedoColor = new Color(_waterProfile.SurfaceDark.R,
                    _waterProfile.SurfaceDark.G, _waterProfile.SurfaceDark.B, 0f),
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
    }

    private void MakeWakeDebugView()
    {
        _wakeDebugMesh = new ImmediateMesh();
        _wakeDebugView = new MeshInstance3D
        {
            Name = "WakePathDebug", Mesh = _wakeDebugMesh, TopLevel = true,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                VertexColorUseAsAlbedo = true,
                NoDepthTest = true,
            },
        };
        AddChild(_wakeDebugView);
    }

    private void DrawWakeDebug(PondWakeContacts contact)
    {
        if (_wakeDebugMesh is null) return;
        _wakeDebugMesh.ClearSurfaces();
        _wakeDebugMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        // Display lift only: sample records keep the true local surface height.
        static Vector3 Raised(Vector3 point) => point + Vector3.Up * .06f;
        void Line(Vector3 a, Vector3 b, Color color)
        {
            _wakeDebugMesh.SurfaceSetColor(color);
            _wakeDebugMesh.SurfaceAddVertex(Raised(a));
            _wakeDebugMesh.SurfaceSetColor(color);
            _wakeDebugMesh.SurfaceAddVertex(Raised(b));
        }
        var center = (contact.Left + contact.Right) * .5f;
        Line(contact.Bow, center, Colors.Yellow);
        Line(contact.Left, contact.Right, contact.Touching ? Colors.Lime : Colors.Red);
        Line(center, contact.Tail, Colors.Yellow);
        Line(center, center + contact.Heading * Mathf.Max(.08f, _contactBody.Length * .3f),
            Colors.Cyan);
        for (var i = 0; i < _wakePath.Count; i++)
        {
            var sample = _wakePath[i];
            var age = _wakePath.Seconds - sample.BirthSeconds;
            var fade = 1f - Mathf.Clamp(age / PondWakePath.Lifetime, 0f, 1f);
            var outward = new Vector2(-sample.Heading.Y, sample.Heading.X) *
                sample.Width * .65f * age / PondWakePath.Lifetime;
            var y = _fish.PondSurfaceHeightAt(sample.Center.X, sample.Center.Y);
            var centerPoint = new Vector3(sample.Center.X, y, sample.Center.Y);
            var leftPoint = new Vector3(sample.Left.X - outward.X,
                _fish.PondSurfaceHeightAt(sample.Left.X - outward.X, sample.Left.Y - outward.Y),
                sample.Left.Y - outward.Y);
            var rightPoint = new Vector3(sample.Right.X + outward.X,
                _fish.PondSurfaceHeightAt(sample.Right.X + outward.X, sample.Right.Y + outward.Y),
                sample.Right.Y + outward.Y);
            var color = new Color(.2f, .85f, 1f, fade);
            if (i > 0 && _wakePath[i - 1].Segment == sample.Segment)
            {
                var prior = _wakePath[i - 1];
                var priorY = _fish.PondSurfaceHeightAt(prior.Center.X, prior.Center.Y);
                Line(new Vector3(prior.Center.X, priorY, prior.Center.Y), centerPoint, color);
                var priorAge = _wakePath.Seconds - prior.BirthSeconds;
                var priorOutward = new Vector2(-prior.Heading.Y, prior.Heading.X) *
                    prior.Width * .65f * priorAge / PondWakePath.Lifetime;
                var priorLeft = prior.Left - priorOutward;
                var priorRight = prior.Right + priorOutward;
                Line(new Vector3(priorLeft.X, _fish.PondSurfaceHeightAt(priorLeft.X, priorLeft.Y),
                    priorLeft.Y), leftPoint, Colors.Magenta * fade);
                Line(new Vector3(priorRight.X, _fish.PondSurfaceHeightAt(priorRight.X, priorRight.Y),
                    priorRight.Y), rightPoint, Colors.Green * fade);
            }
            Line(centerPoint, leftPoint, Colors.Magenta * fade);
            Line(centerPoint, rightPoint, Colors.Green * fade);
        }
        _wakeDebugMesh.SurfaceEnd();
    }

    /// <summary>Development probe refresh after a staged diagnostic pose.</summary>
    public void RefreshWakeDebug()
    {
        if (_wakeDebugMesh is null) return;
        DrawWakeDebug(PondWakeContacts.At(_contactBody, _fish.GlobalTransform,
            _fish.PondSurfaceHeightAt));
    }

    /// <summary>Native diagnostic redraw after a controlled historical route.</summary>
    public void RefreshWakeRenderForProbe()
    {
        if (!OS.IsDebugBuild() || _ribbonMesh is null || _ribbonMaterial is null ||
            _pondAtmosphere is null) return;
        ActiveWakeTriangles = PondWakeRibbon.Draw(_ribbonMesh, _wakePath, _wakePath.Seconds,
            _pondAtmosphere.GetRenderedSurfaceHeight, ShoreDistance, _ribbonMaterial,
            _lowSurfaceEffects ? .5f : 1f, _latestContact, _latestContactEnergy);
    }

    /// <summary>Native fixed-route comparison only; never changes the motor.</summary>
    public void SetWakeProbeContact(PondWakeContacts contact, float energy)
    {
        if (!OS.IsDebugBuild()) return;
        _latestContact = contact.Touching && energy > .03f ? contact : null;
        _latestContactEnergy = energy;
    }

    /// <summary>Debug-only A/B toggle. Production never allocates the ring pool.</summary>
    public void SetWakeLegacyComparison(bool legacy)
    {
        if (!OS.IsDebugBuild() || !_legacyRings) return;
        _showLegacyRings = legacy;
        if (legacy)
        {
            _ribbonMesh?.ClearSurfaces();
            ActiveWakeTriangles = 0;
        }
        else
            foreach (var ring in _rings) if (ring is not null) ring.Visible = false;
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
        _wakePath.Suspend();
        _latestContact = null;
        _latestContactEnergy = 0f;
        if (!IsNodeReady()) return;
        Clear();
        _ribbonMesh?.ClearSurfaces();
        ActiveWakeTriangles = 0;
        for (var i = 0; i < PondRingCapacity; i++)
            if (_rings[i] is not null) { _rings[i].Visible = false; _ringAges[i] = 0f; }
        _surfaceClock = 0f;
        _splashCooldown = 0f;
        if (_splash is not null) { _splash.Restart(); _splash.Emitting = false; }
        _lastSpeed = _fish.PresentationSpeed;
        _boostBlend = 0f;
        _boostVisualStep = -1;
        _suspended = true;
        _wakeDebugMesh?.ClearSurfaces();
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
