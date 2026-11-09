#nullable enable
using Godot;

namespace Fish;

/// <summary>Samples the imported Fish3 swim clip and adds bounded visual-only turning.</summary>
public partial class FishVisualMotion : Node
{
    private const string SwimClip = "Armature|Swim";
    private const float SeamBlendStart = 0.88f;

    private readonly FishController _fish;
    private readonly Node3D _visual;
    private readonly Skeleton3D _skeleton;
    private readonly AnimationPlayer _player;
    private readonly Animation _clip;
    private readonly int[] _bones;
    private readonly int[] _tracks;
    private readonly Quaternion[] _neutral;
    private Quaternion _visualBaseRotation;
    private double _phaseSeconds;
    private float _activity;
    private float _turn;

    public float Activity => _activity;
    public float TurnBlend => _turn;
    public double PhaseSeconds => _phaseSeconds;
    public Vector3 GetPresentationState() => new(_activity, _turn, (float)_phaseSeconds);

    private FishVisualMotion(FishController fish, Node3D visual, Skeleton3D skeleton,
        AnimationPlayer player, Animation clip, int[] bones, int[] tracks)
    {
        _fish = fish;
        _visual = visual;
        _skeleton = skeleton;
        _player = player;
        _clip = clip;
        _bones = bones;
        _tracks = tracks;
        _neutral = new Quaternion[tracks.Length];
        for (var i = 0; i < tracks.Length; i++)
            _neutral[i] = clip.RotationTrackInterpolate(tracks[i], 0).Normalized();
        Name = "SwimPresentation";
    }

    public static FishVisualMotion? CreateVerified(FishController fish, Node3D visual,
        Skeleton3D skeleton, AnimationPlayer player, Animation clip)
    {
        var names = new[] { "Root", "Spine3", "Tail" };
        var bones = new int[names.Length];
        var tracks = new int[names.Length];
        for (var i = 0; i < names.Length; i++)
        {
            bones[i] = skeleton.FindBone(names[i]);
            tracks[i] = -1;
            for (var track = 0; track < clip.GetTrackCount(); track++)
            {
                if (clip.TrackGetType(track) != Animation.TrackType.Rotation3D) return null;
                if (clip.TrackGetPath(track).ToString() == $"Armature/Skeleton3D:{names[i]}")
                    tracks[i] = track;
            }
            if (bones[i] < 0 || tracks[i] < 0 || clip.TrackGetKeyCount(tracks[i]) < 2)
                return null;
        }
        // This player uses precisely the three inspected bone-rotation tracks.
        if (clip.GetTrackCount() != tracks.Length || clip.Length <= 0) return null;
        return new FishVisualMotion(fish, visual, skeleton, player, clip, bones, tracks);
    }

    public override void _Ready()
    {
        _visualBaseRotation = _visual.Quaternion;
        _player.CallbackModeProcess = AnimationMixer.AnimationCallbackModeProcess.Manual;
        _player.Play(SwimClip);
        ResetPresentation();
    }

    public void ResetPresentation()
    {
        _phaseSeconds = 0;
        _activity = 0;
        _turn = 0;
        ApplyPose();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (delta <= 0 || GetTree().Paused) return;
        var tuning = _fish.Tuning;
        var speed = Mathf.Clamp(_fish.PresentationSpeed / Mathf.Max(0.001f, tuning.MaxSpeed), 0, 1);
        var effort = Mathf.Clamp(_fish.PresentationEffort, 0, 1);
        var desiredActivity = effort > 0
            ? Mathf.Clamp(tuning.SwimIntensity * effort, 0, 1)
            : Mathf.Clamp(tuning.GlideIntensity * speed, 0, 1);
        var activityResponse = desiredActivity > _activity
            ? tuning.SwimActivityResponse : tuning.GlideEasingResponse;
        _activity = Mathf.Lerp(_activity, desiredActivity,
            1 - Mathf.Exp(-Mathf.Max(0.01f, activityResponse) * (float)delta));
        _turn = Mathf.Lerp(_turn, _fish.PresentationTurn,
            1 - Mathf.Exp(-Mathf.Max(0.01f, tuning.TurnResponse) * (float)delta));

        var cadence = Mathf.Lerp(tuning.GlideCadenceHz, tuning.SwimCadenceHz, _activity);
        cadence *= 0.45f + 0.55f * speed;
        _phaseSeconds = (_phaseSeconds + _clip.Length * Mathf.Max(0, cadence) * delta) % _clip.Length;
        ApplyPose();
    }

    private void ApplyPose()
    {
        _player.Seek(_phaseSeconds, true, true);
        for (var i = 0; i < _bones.Length; i++)
        {
            var sampled = _clip.RotationTrackInterpolate(_tracks[i], _phaseSeconds).Normalized();
            var seamAt = _clip.Length * SeamBlendStart;
            if (_phaseSeconds > seamAt)
            {
                var blend = (float)((_phaseSeconds - seamAt) / (_clip.Length - seamAt));
                blend = blend * blend * (3 - 2 * blend);
                sampled = sampled.Slerp(_neutral[i], blend);
            }
            var pose = _neutral[i].Slerp(sampled, _activity);
            if (i == 1 || i == 2)
            {
                var bend = Mathf.DegToRad(_fish.Tuning.TurnBendDegrees) * _turn *
                    (i == 2 ? 1.4f : 1.0f);
                pose *= new Quaternion(Vector3.Forward, bend);
            }
            _skeleton.SetBonePoseRotation(_bones[i], pose.Normalized());
        }
        var lean = Mathf.DegToRad(_fish.Tuning.TurnLeanDegrees) * _turn;
        _visual.Quaternion = _visualBaseRotation * new Quaternion(Vector3.Right, lean);
    }
}
