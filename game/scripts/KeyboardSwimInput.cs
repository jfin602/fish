using Godot;
using System.Collections.Generic;

namespace Fish;

/// <summary>Physical controls for one active preset. Events become one physics intent.</summary>
public sealed class ProfileSwimInput
{
    private readonly HashSet<int> _keys = [];
    private readonly HashSet<int> _buttons = [];
    private readonly HashSet<int> _blockedKeys = [];
    private readonly HashSet<int> _blockedButtons = [];
    private readonly MouseSwimInput _mouse = new();
    private ControlPreset _profile = ControlPreset.Builtins()[0];

    public ControlPreset Profile => _profile;
    public bool AllReleased => _keys.Count == 0 && _buttons.Count == 0 &&
        _blockedKeys.Count == 0 && _blockedButtons.Count == 0;
    public int HeldCount => _keys.Count + _buttons.Count;
    public bool AdaptiveFreeLookHeld => _profile.CameraControl == CameraControlPolicy.AdaptiveFollow &&
        _keys.Contains(ControlPreset.AdaptiveFreeLookKey);
    public bool UsesMouse => _profile.CameraControl == CameraControlPolicy.MouseOrbit360 ||
        _profile.MouseX.Steering != SteeringAxis.None ||
        _profile.MouseY.Steering != SteeringAxis.None || HasMouseBinding();
    public bool UsesKeyboardSteering => HasKeyBinding(ControlAction.Thrust) ||
        HasKeyBinding(ControlAction.Slow) || HasKeyBinding(ControlAction.YawLeft) ||
        HasKeyBinding(ControlAction.YawRight) || HasKeyBinding(ControlAction.PitchUp) ||
        HasKeyBinding(ControlAction.PitchDown);

    public void Select(ControlPreset profile)
    {
        _profile = profile;
        _mouse.Clear();
    }

    public void ClearQueued()
    {
        _blockedKeys.UnionWith(_keys);
        _blockedButtons.UnionWith(_buttons);
        _keys.Clear();
        _buttons.Clear();
        _mouse.Clear();
    }
    public void CaptureStarted() => _mouse.OnCaptureStarted();

    public void Accept(InputEvent @event, bool gameplayMouseCaptured)
    {
        if (@event is InputEventKey key && !key.Echo)
        {
            var code = (int)key.PhysicalKeycode;
            if (code == 0) code = (int)key.Keycode;
            if (code == ControlPreset.AdaptiveFreeLookKey &&
                _profile.CameraControl == CameraControlPolicy.AdaptiveFollow)
                _mouse.DiscardAdaptiveSteering();
            if (key.Pressed)
            {
                if (!_blockedKeys.Contains(code)) _keys.Add(code);
            }
            else
            {
                _keys.Remove(code);
                _blockedKeys.Remove(code);
            }
        }
        else if (@event is InputEventMouseButton button && button.ButtonIndex is not
                 (MouseButton.WheelUp or MouseButton.WheelDown or MouseButton.WheelLeft or MouseButton.WheelRight))
        {
            var code = (int)button.ButtonIndex;
            if (button.Pressed)
            {
                if (!_blockedButtons.Contains(code)) _buttons.Add(code);
            }
            else
            {
                _buttons.Remove(code);
                _blockedButtons.Remove(code);
            }
        }
        if (gameplayMouseCaptured) _mouse.Accept(@event);
    }

    public SwimIntent Consume(float delta, SwimTuning tuning, bool captured,
        float sensitivity, bool invertY)
    {
        if (AdaptiveFreeLookHeld) _mouse.DiscardAdaptiveSteering();
        var steering = captured && !AdaptiveFreeLookHeld
            ? _mouse.ConsumeSteering(delta, tuning, _profile, sensitivity, invertY)
            : Vector2.Zero;
        var yaw = (IsDown(ControlAction.YawRight, captured) ? 1 : 0) - (IsDown(ControlAction.YawLeft, captured) ? 1 : 0);
        var pitch = (IsDown(ControlAction.PitchUp, captured) ? 1 : 0) - (IsDown(ControlAction.PitchDown, captured) ? 1 : 0);
        if (Input.IsKeyPressed(Key.Shift) && !UsesMouse) pitch = 0; // Legacy keyboard camera hold.
        if (_profile.CameraControl == CameraControlPolicy.AdaptiveFollow)
            steering = new Vector2(yaw == 0 ? steering.X : 0, pitch == 0 ? steering.Y : 0);
        return new SwimIntent(IsDown(ControlAction.Thrust, captured) ? 1 : 0,
            IsDown(ControlAction.Slow, captured) ? 1 : 0,
            Mathf.Clamp(yaw + steering.X, -1, 1), Mathf.Clamp(pitch + steering.Y, -1, 1));
    }

    public bool IsDown(ControlAction action, bool captured)
    {
        foreach (var binding in _profile.Bindings(action))
            if (binding.Device == BindingDevice.PhysicalKey ? _keys.Contains(binding.Code) :
                captured && _buttons.Contains(binding.Code)) return true;
        return false;
    }

    private bool HasKeyBinding(ControlAction action)
    {
        foreach (var binding in _profile.Bindings(action))
            if (binding.Device == BindingDevice.PhysicalKey) return true;
        return false;
    }

    private bool HasMouseBinding()
    {
        foreach (var action in System.Enum.GetValues<ControlAction>())
        {
            if (action == ControlAction.PauseOptions) continue;
            foreach (var binding in _profile.Bindings(action))
                if (binding.Device == BindingDevice.MouseButton) return true;
        }
        return false;
    }
}
