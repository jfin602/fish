#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace Fish;

/// <summary>Encounter-only presentation of committed fish predation events.</summary>
public partial class FishBloodFx : Node3D
{
    public const int Capacity = 4;
    public const int ParticlesPerCloud = 12;
    public const float DefaultDuration = 1.5f;
    private readonly HashSet<string> _seen = new(StringComparer.Ordinal);
    private readonly GpuParticles3D[] _emitters = new GpuParticles3D[Capacity];
    private readonly StandardMaterial3D[] _materials = new StandardMaterial3D[Capacity];
    private readonly float[] _ages = new float[Capacity];
    private readonly string[] _eventIds = new string[Capacity];
    private bool _enabled = true;
    private bool _lowGraphics;
    private bool _paused;

    [Export(PropertyHint.Range, "0.8,2.5,0.1")]
    public float DurationSeconds { get; set; } = DefaultDuration;
    public int ActiveCount { get; private set; }
    public int EmissionCount { get; private set; }
    public int ParticleBudget => Capacity * (_lowGraphics ? 6 : ParticlesPerCloud);
    public bool Enabled => _enabled;
    public bool LowGraphics => _lowGraphics;
    public bool Paused => _paused;
    public IReadOnlyCollection<string> SeenEventIds => _seen;

    public override void _Ready()
    {
        var softDisc = MakeSoftDisc();
        for (var i = 0; i < Capacity; i++)
        {
            _eventIds[i] = "";
            var material = new StandardMaterial3D
            {
                AlbedoColor = new Color(.48f, .12f, .075f, 0f),
                AlbedoTexture = softDisc,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
                CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            };
            var process = new ParticleProcessMaterial
            {
                EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere,
                EmissionSphereRadius = .18f,
                Direction = Vector3.Up,
                Spread = 180f,
                Gravity = new Vector3(0f, .025f, 0f),
                InitialVelocityMin = .22f,
                InitialVelocityMax = .48f,
                ScaleMin = .7f,
                ScaleMax = 1.4f,
            };
            var emitter = new GpuParticles3D
            {
                Name = $"BloodCloud{i}",
                TopLevel = true,
                OneShot = true,
                Emitting = false,
                Amount = ParticlesPerCloud,
                Lifetime = DurationSeconds,
                LocalCoords = false,
                VisibilityAabb = new Aabb(new Vector3(-1.2f, -1.2f, -1.2f),
                    new Vector3(2.4f, 2.4f, 2.4f)),
                ProcessMaterial = process,
                DrawPass1 = new QuadMesh
                {
                    Size = new Vector2(.28f, .28f),
                    Material = material,
                },
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                Visible = false,
            };
            AddChild(emitter);
            _emitters[i] = emitter;
            _materials[i] = material;
        }
    }

    /// <summary>Returns false for disabled effects, duplicate events, or a full pool.</summary>
    public bool TryEmit(string eventId, Vector3 contact)
    {
        if (string.IsNullOrWhiteSpace(eventId) || _seen.Contains(eventId)) return false;
        // Remember even suppressed events: changing settings cannot replay an old bite.
        _seen.Add(eventId);
        if (!_enabled || _paused || !IsInsideTree() ||
            !float.IsFinite(contact.X) || !float.IsFinite(contact.Y) ||
            !float.IsFinite(contact.Z)) return false;
        for (var i = 0; i < Capacity; i++)
        {
            if (_eventIds[i] != "") continue;
            _eventIds[i] = eventId;
            _ages[i] = 0f;
            var emitter = _emitters[i];
            emitter.GlobalPosition = contact;
            emitter.Amount = _lowGraphics ? 6 : ParticlesPerCloud;
            emitter.Lifetime = DurationSeconds;
            emitter.SpeedScale = Engine.TimeScale;
            emitter.Visible = true;
            _materials[i].AlbedoColor = new Color(.48f, .12f, .075f, .58f);
            emitter.Restart();
            emitter.Emitting = true;
            ActiveCount++;
            EmissionCount++;
            return true;
        }
        return false;
    }

    public override void _Process(double delta)
    {
        if (_paused || delta <= 0) return;
        for (var i = 0; i < Capacity; i++)
        {
            if (_eventIds[i] == "") continue;
            _ages[i] += (float)delta;
            if (_ages[i] >= DurationSeconds) { Stop(i); continue; }
            var t = Mathf.Clamp(_ages[i] / DurationSeconds, 0f, 1f);
            // Dark rust at contact diffuses toward the Pond's muted green/brown.
            var color = new Color(.48f, .12f, .075f).Lerp(
                new Color(.25f, .27f, .16f), t);
            color.A = .58f * (1f - Mathf.SmoothStep(0f, 1f, t));
            _materials[i].AlbedoColor = color;
            _emitters[i].SpeedScale = Engine.TimeScale;
        }
    }

    public void SetPaused(bool paused)
    {
        _paused = paused;
        foreach (var emitter in _emitters)
            if (emitter is not null) emitter.SpeedScale = paused ? 0f : Engine.TimeScale;
    }

    public void Configure(bool enabled, bool lowGraphics)
    {
        _enabled = enabled;
        _lowGraphics = lowGraphics;
        if (!enabled) Clear();
        else for (var i = 0; i < Capacity; i++)
            if (_emitters[i] is not null) _emitters[i].Amount = lowGraphics ? 6 : ParticlesPerCloud;
    }

    public void Clear()
    {
        for (var i = 0; i < Capacity; i++) Stop(i);
    }

    private void Stop(int i)
    {
        if (_emitters[i] is null) return;
        if (_eventIds[i] != "") ActiveCount--;
        _eventIds[i] = "";
        _ages[i] = 0f;
        _emitters[i].Emitting = false;
        _emitters[i].Visible = false;
        _emitters[i].Restart();
        _emitters[i].Emitting = false;
        _materials[i].AlbedoColor = new Color(.48f, .12f, .075f, 0f);
    }

    private static ImageTexture MakeSoftDisc()
    {
        var image = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        for (var y = 0; y < 32; y++)
        for (var x = 0; x < 32; x++)
        {
            var radius = new Vector2((x + .5f) / 16f - 1f,
                (y + .5f) / 16f - 1f).Length();
            var edge = Mathf.Clamp(1f - radius, 0f, 1f);
            image.SetPixel(x, y, new Color(1f, 1f, 1f, edge * Mathf.Sqrt(edge)));
        }
        return ImageTexture.CreateFromImage(image);
    }
}
