using Godot;

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
    private readonly FishController _fish;
    private GpuParticles3D _left = null!;
    private GpuParticles3D _right = null!;
    private GpuParticles3D _bubbles = null!;
    private bool _enabled = true;
    private bool _suspended = true;
    private float _lastSpeed;

    public WaterReaction(FishController fish)
    {
        _fish = fish;
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
