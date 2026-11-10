#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

// These are requests, not input events or motor commands. The P2 resolver owns event dispatch.
public enum ControlAction { Thrust, Slow, YawLeft, YawRight, PitchUp, PitchDown, PauseOptions }
public enum BindingDevice { PhysicalKey, MouseButton }
public enum SteeringAxis { None, Yaw, Pitch }
public readonly record struct ControlBinding(BindingDevice Device, int Code);
public readonly record struct MouseAxisAssignment(SteeringAxis Steering, bool Inverted = false);

/// <summary>Validated snapshot of one desktop control profile. No gameplay state lives here.</summary>
public sealed class ControlPreset
{
    public const int SchemaVersion = 1;
    public const int MaxBindingsPerAction = 4;
    public const int MaxNameLength = 32;
    public const float DefaultSensitivity = 0.12f;
    public const string KeyboardId = "builtin:keyboard";
    public const string MouseId = "builtin:mouse";
    public const string HybridId = "builtin:hybrid";
    public const int RecoveryKey = (int)Key.Escape;
    public const int RecoveryMouseButton = (int)MouseButton.Middle;
    private static readonly ControlAction[] AllActions = Enum.GetValues<ControlAction>();
    private readonly Dictionary<ControlAction, ControlBinding[]> _bindings;

    public string Id { get; }
    public string Name { get; }
    public bool IsBuiltin => Id.StartsWith("builtin:", StringComparison.Ordinal);
    public MouseAxisAssignment MouseX { get; }
    public MouseAxisAssignment MouseY { get; }
    public float MouseSensitivity { get; }
    public IReadOnlyList<ControlBinding> Bindings(ControlAction action) =>
        Array.AsReadOnly(_bindings[action]);

    public ControlPreset(string id, string name, IDictionary<ControlAction, ControlBinding[]> bindings,
        MouseAxisAssignment mouseX, MouseAxisAssignment mouseY, float mouseSensitivity)
    {
        if (!IsValidId(id)) throw new ArgumentException("Invalid profile identity", nameof(id));
        if (string.IsNullOrWhiteSpace(name) || name.Trim() != name || name.Length > MaxNameLength ||
            name.Any(char.IsControl)) throw new ArgumentException("Invalid profile name", nameof(name));
        if (!float.IsFinite(mouseSensitivity) || mouseSensitivity < 0.04f || mouseSensitivity > 0.4f)
            throw new ArgumentOutOfRangeException(nameof(mouseSensitivity));
        if (!Enum.IsDefined(mouseX.Steering) || !Enum.IsDefined(mouseY.Steering) ||
            (mouseX.Steering != SteeringAxis.None && mouseX.Steering == mouseY.Steering))
            throw new ArgumentException("Conflicting mouse steering axes");
        if (bindings.Count != AllActions.Length || bindings.Keys.Any(action => !Enum.IsDefined(action)))
            throw new ArgumentException("All semantic actions must be present", nameof(bindings));

        var seen = new HashSet<ControlBinding>();
        _bindings = new();
        foreach (var action in AllActions)
        {
            if (!bindings.TryGetValue(action, out var entries) || entries is null ||
                entries.Length > MaxBindingsPerAction) throw new ArgumentException("Invalid action binding count");
            foreach (var entry in entries)
            {
                if (!ValidBinding(entry) ||
                    ((entry.Device == BindingDevice.PhysicalKey && entry.Code == RecoveryKey) ||
                     (entry.Device == BindingDevice.MouseButton && entry.Code == RecoveryMouseButton)) &&
                    action != ControlAction.PauseOptions)
                    throw new ArgumentException("Invalid or reserved binding");
                if (!seen.Add(entry)) throw new ArgumentException("Physical binding conflicts between actions");
            }
            _bindings[action] = (ControlBinding[])entries.Clone();
        }
        if (_bindings[ControlAction.Thrust].Length == 0 || _bindings[ControlAction.Slow].Length == 0)
            throw new ArgumentException("Swim and Slow each need a binding");
        // Escape is a fixed recovery input, independent of user capture and editable bindings.
        // The visible Options button is the second recovery path in PlayShell.
        Id = id;
        Name = name;
        MouseX = mouseX;
        MouseY = mouseY;
        MouseSensitivity = mouseSensitivity;
    }

    public static bool IsValidId(string? id)
    {
        if (id is KeyboardId or MouseId or HybridId) return true;
        return id is not null && id.StartsWith("custom:", StringComparison.Ordinal) &&
            Guid.TryParseExact(id[7..], "N", out var value) && value != Guid.Empty &&
            id == "custom:" + value.ToString("N");
    }

    private static bool ValidBinding(ControlBinding binding) => binding.Device switch
    {
        BindingDevice.PhysicalKey => Enum.IsDefined((Key)binding.Code) &&
            binding.Code != (int)Key.None && binding.Code != (int)Key.Shift &&
            binding.Code != (int)Key.Ctrl && binding.Code != (int)Key.Alt && binding.Code != (int)Key.Meta,
        BindingDevice.MouseButton => binding.Code is (int)MouseButton.Left or (int)MouseButton.Right or
            (int)MouseButton.Middle or (int)MouseButton.Xbutton1 or (int)MouseButton.Xbutton2,
        _ => false
    };

    public ControlPreset Copy(string id, string name) =>
        new(id, name, _bindings, MouseX, MouseY, MouseSensitivity);
    public ControlPreset WithBindings(ControlAction action, params ControlBinding[] entries)
    {
        var copy = _bindings.ToDictionary(pair => pair.Key, pair => pair.Value);
        copy[action] = entries;
        return new(Id, Name, copy, MouseX, MouseY, MouseSensitivity);
    }
    public ControlPreset WithMouse(MouseAxisAssignment x, MouseAxisAssignment y, float sensitivity) =>
        new(Id, Name, _bindings, x, y, sensitivity);
    public ControlPreset WithName(string name) => new(Id, name, _bindings, MouseX, MouseY, MouseSensitivity);

    private static ControlBinding KeyBinding(Key key) => new(BindingDevice.PhysicalKey, (int)key);
    private static ControlBinding ButtonBinding(MouseButton button) => new(BindingDevice.MouseButton, (int)button);
    private static Dictionary<ControlAction, ControlBinding[]> EmptyActions() =>
        AllActions.ToDictionary(action => action, _ => Array.Empty<ControlBinding>());

    public static IReadOnlyList<ControlPreset> Builtins()
    {
        var keyboard = EmptyActions();
        keyboard[ControlAction.Thrust] = [KeyBinding(Key.W)];
        keyboard[ControlAction.Slow] = [KeyBinding(Key.S)];
        keyboard[ControlAction.YawLeft] = [KeyBinding(Key.A)];
        keyboard[ControlAction.YawRight] = [KeyBinding(Key.D)];
        keyboard[ControlAction.PitchUp] = [KeyBinding(Key.Up)];
        keyboard[ControlAction.PitchDown] = [KeyBinding(Key.Down)];
        keyboard[ControlAction.PauseOptions] = [KeyBinding(Key.Escape)];
        var mouse = EmptyActions();
        mouse[ControlAction.Thrust] = [ButtonBinding(MouseButton.Left)];
        mouse[ControlAction.Slow] = [ButtonBinding(MouseButton.Right)];
        mouse[ControlAction.PauseOptions] = [KeyBinding(Key.Escape), ButtonBinding(MouseButton.Middle)];
        var hybrid = EmptyActions();
        hybrid[ControlAction.Thrust] = [KeyBinding(Key.W), ButtonBinding(MouseButton.Left)];
        hybrid[ControlAction.Slow] = [KeyBinding(Key.S), ButtonBinding(MouseButton.Right)];
        hybrid[ControlAction.YawLeft] = [KeyBinding(Key.A)];
        hybrid[ControlAction.YawRight] = [KeyBinding(Key.D)];
        hybrid[ControlAction.PitchUp] = [KeyBinding(Key.Up)];
        hybrid[ControlAction.PitchDown] = [KeyBinding(Key.Down)];
        hybrid[ControlAction.PauseOptions] = [KeyBinding(Key.Escape), ButtonBinding(MouseButton.Middle)];
        return Array.AsReadOnly(new[]
        {
            new ControlPreset(KeyboardId, "Keyboard Only", keyboard, default, default, DefaultSensitivity),
            new ControlPreset(MouseId, "Mouse Only", mouse, new(SteeringAxis.Yaw), new(SteeringAxis.Pitch), DefaultSensitivity),
            new ControlPreset(HybridId, "Hybrid Swim", hybrid, new(SteeringAxis.Yaw), new(SteeringAxis.Pitch), DefaultSensitivity)
        });
    }
}
