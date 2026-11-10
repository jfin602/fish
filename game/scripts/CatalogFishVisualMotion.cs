using Godot;

namespace Fish;

/// <summary>Rig-independent playback of each imported model's own animation clips.</summary>
public partial class CatalogFishVisualMotion : Node
{
    private readonly FishController _fish;
    private readonly AnimationPlayer _player;
    private readonly CharacterEntry _entry;
    private string _active = "";
    private float _activity;

    public float Activity => _activity;
    public string ActiveClip => _active;

    public CatalogFishVisualMotion(FishController fish, AnimationPlayer player, CharacterEntry entry)
    {
        _fish = fish;
        _player = player;
        _entry = entry;
        Name = "SwimPresentation";
    }

    public override void _Ready() => Play(_entry.GlideClip.Length > 0 ? _entry.GlideClip : _entry.SwimClip);

    public override void _PhysicsProcess(double delta)
    {
        if (delta <= 0 || GetTree().Paused) return;
        var effort = Mathf.Clamp(_fish.PresentationEffort, 0, 1);
        var speed = Mathf.Clamp(_fish.PresentationSpeed / Mathf.Max(.001f, _fish.Tuning.MaxSpeed), 0, 1);
        var target = effort > .02f ? effort : speed * .2f;
        _activity = Mathf.Lerp(_activity, target, 1f - Mathf.Exp(-3f * (float)delta));
        var desired = _entry.GlideClip;
        if (desired.Length == 0) desired = _entry.SwimClip;
        else if (_activity > .70f && _entry.FastClip.Length > 0) desired = _entry.FastClip;
        else if (_activity > .24f) desired = _entry.SwimClip;
        if (desired != _active) Play(desired);
        _player.SpeedScale = _entry.GlideClip.Length == 0
            ? Mathf.Lerp(.38f, 1.35f, _activity)
            : Mathf.Lerp(.7f, 1.2f, _activity);
        // FBX imports are nonlooping. Restart the source clip without assuming a
        // shared bone hierarchy or modifying imported resources used elsewhere.
        var clip = _player.GetAnimation(_active);
        if (clip is not null && clip.LoopMode == Animation.LoopModeEnum.None &&
            _player.CurrentAnimationPosition >= clip.Length - (float)delta * _player.SpeedScale)
            _player.Seek(0, true);
    }

    private void Play(string clip)
    {
        _active = clip;
        _player.Play(clip, .18);
    }
}
