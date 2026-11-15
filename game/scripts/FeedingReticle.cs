#nullable enable
using Godot;
using System;

namespace Fish;

/// <summary>Presentation-only view reference and projected physical mouth heading.</summary>
public partial class FeedingReticle : Control
{
    private readonly FishController _fish;
    private readonly Camera3D _camera;
    private readonly Func<bool> _sessionAllowsHud;
    private Vector2 _heading;
    private bool _headingVisible;
    private bool _headingInitialized;
    private float _pulseSeconds;

    public int ConfirmedFeeds { get; private set; }
    public bool HeadingVisible => _headingVisible;
    public Vector2 HeadingPosition => _heading;

    public FeedingReticle(FishController fish, Camera3D camera, Func<bool> sessionAllowsHud)
    {
        _fish = fish;
        _camera = camera;
        _sessionAllowsHud = sessionAllowsHud;
        Name = "FeedingReticle";
        MouseFilter = MouseFilterEnum.Ignore;
        ProcessMode = ProcessModeEnum.Always;
        ProcessPriority = 100; // Read the camera after its render-time interpolation.
    }

    /// <summary>Reusable in the fixture and the future normal-Pond Bass session.</summary>
    public static FeedingReticle Attach(Node owner, FishController fish, Camera3D camera,
        Func<bool> sessionAllowsHud)
    {
        var layer = new CanvasLayer { Name = "FeedingReticleLayer", Layer = 9,
            ProcessMode = ProcessModeEnum.Always };
        owner.AddChild(layer);
        var reticle = new FeedingReticle(fish, camera, sessionAllowsHud);
        layer.AddChild(reticle);
        reticle.SetAnchorsPreset(LayoutPreset.FullRect);
        return reticle;
    }

    public void ConfirmCommittedFeed()
    {
        // Called only after the authoritative ledger returns Consumed=true.
        ConfirmedFeeds++;
        if (Visible) _pulseSeconds = .24f;
        QueueRedraw();
    }

    public void ClearPresentation()
    {
        _pulseSeconds = 0f;
        _headingVisible = false;
        _headingInitialized = false;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        // A CanvasLayer is not a Control parent; full-rect anchors alone have no size.
        var viewportSize = GetViewport().GetVisibleRect().Size;
        if (Size != viewportSize) Size = viewportSize;
        var allowed = ShouldShow(_sessionAllowsHud(), _fish.Visible,
            _fish.IsPhysicsProcessing(), _fish.GetInputArmed(),
            _fish.GetTree().Paused, _camera.Current);
        if (!allowed) { ClearPresentation(); return; }
        Visible = true;
        _pulseSeconds = Mathf.Max(0f, _pulseSeconds - (float)delta);
        var fishPose = _fish.GetGlobalTransformInterpolated();
        var mouth = fishPose.Origin - fishPose.Basis.Z * _fish.BodyForwardReach;
        var forward = -fishPose.Basis.Z;
        var rayDistance = Mathf.Max(.8f, _fish.BodyForwardReach * 4f);
        if (TryProjectHeading(_camera, mouth, forward, rayDistance, Size,
                out var projected) && projected.DistanceTo(Size * .5f) > 12f * UiScale)
        {
            _heading = _headingInitialized
                ? _heading.Lerp(projected, 1f - Mathf.Exp(-18f * (float)delta))
                : projected;
            _headingVisible = _headingInitialized = true;
        }
        else
        {
            _headingVisible = false;
            _headingInitialized = false;
        }
        QueueRedraw();
    }

    public static bool ShouldShow(bool sessionAllowsHud, bool bodyVisible,
        bool motorRunning, bool inputArmed, bool paused, bool cameraCurrent) =>
        sessionAllowsHud && bodyVisible && motorRunning && inputArmed &&
        !paused && cameraCurrent;

    public static bool TryProjectHeading(Camera3D camera, Vector3 mouth,
        Vector3 forward, float distance, Vector2 viewportSize, out Vector2 screen)
    {
        screen = default;
        if (!forward.IsNormalized() || distance <= 0f ||
            viewportSize.X <= 0f || viewportSize.Y <= 0f) return false;
        var ahead = mouth + forward * distance;
        if (camera.IsPositionBehind(ahead)) return false;
        screen = camera.UnprojectPosition(ahead);
        var margin = 14f;
        return float.IsFinite(screen.X) && float.IsFinite(screen.Y) &&
            screen.X >= margin && screen.Y >= margin &&
            screen.X <= viewportSize.X - margin &&
            screen.Y <= viewportSize.Y - margin;
    }

    private float UiScale => Mathf.Clamp(Mathf.Min(Size.X / 1600f, Size.Y / 900f), .7f, 1.4f);

    public override void _Draw()
    {
        if (!Visible) return;
        var center = Size * .5f;
        var scale = UiScale;
        var dark = new Color(.01f, .06f, .08f, .78f);
        var light = new Color(.94f, .98f, .95f, .86f);
        for (var axis = 0; axis < 4; axis++)
        {
            var unit = axis switch
            {
                0 => Vector2.Up, 1 => Vector2.Down,
                2 => Vector2.Left, _ => Vector2.Right
            };
            var from = center + unit * 3.5f * scale;
            var to = center + unit * 8f * scale;
            DrawLine(from, to, dark, 3.5f * scale, true);
            DrawLine(from, to, light, 1.5f * scale, true);
        }
        if (_headingVisible)
        {
            var radius = 5f * scale;
            var points = new[] { _heading + Vector2.Up * radius,
                _heading + Vector2.Right * radius, _heading + Vector2.Down * radius,
                _heading + Vector2.Left * radius };
            for (var i = 0; i < points.Length; i++)
            {
                DrawLine(points[i], points[(i + 1) % points.Length], dark, 3.5f * scale, true);
                DrawLine(points[i], points[(i + 1) % points.Length], light, 1.5f * scale, true);
            }
        }
        if (_pulseSeconds > 0f)
        {
            var alpha = Mathf.Clamp(_pulseSeconds / .24f, 0f, 1f) * .75f;
            DrawArc(center, 11f * scale, 0f, Mathf.Tau, 24,
                new Color(.87f, 1f, .85f, alpha), 2f * scale, true);
        }
    }
}
