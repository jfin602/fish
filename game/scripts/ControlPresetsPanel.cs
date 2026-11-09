#nullable enable
using Godot;
using System;
using System.Linq;

namespace Fish;

/// <summary>A paused-menu editor. Drafts do not touch the active resolver until Apply.</summary>
public partial class ControlPresetsPanel : PanelContainer
{
    private readonly ControlPresetStore _store;
    private readonly Func<ControlPreset> _activeProfile;
    private readonly Action<ControlPreset> _apply;
    private readonly Action _close;
    private readonly OptionButton _profiles = new() { Name = "PresetProfileOption" };
    private readonly Label _state = new() { Name = "PresetStateLabel", AutowrapMode = TextServer.AutowrapMode.WordSmart };
    private readonly VBoxContainer _bindings = new() { Name = "PresetBindingRows" };
    private readonly OptionButton _mouseX = new() { Name = "MouseXAxisOption" };
    private readonly OptionButton _mouseY = new() { Name = "MouseYAxisOption" };
    private readonly CheckButton _invertX = new() { Name = "InvertMouseXToggle", Text = "Invert X" };
    private readonly CheckButton _invertY = new() { Name = "InvertMouseYEditorToggle", Text = "Invert Y" };
    private readonly HSlider _sensitivity = new() { Name = "PresetSensitivitySlider", MinValue = 0.04, MaxValue = 0.4, Step = 0.01 };
    private readonly Label _sensitivityText = new();
    private readonly Control _modal = new() { Name = "PresetModal", MouseFilter = MouseFilterEnum.Stop, Visible = false };
    private readonly Label _modalText = new() { Name = "PresetModalText", AutowrapMode = TextServer.AutowrapMode.WordSmart };
    private readonly LineEdit _name = new() { Name = "PresetNameEdit", MaxLength = ControlPreset.MaxNameLength };
    private readonly Button _confirm = new() { Name = "PresetConfirmButton", Text = "Confirm" };
    private readonly Button _cancel = new() { Name = "PresetCancelButton", Text = "Cancel" };
    private readonly Button _back = new() { Name = "PresetBackButton", Text = "Back to Options" };
    private readonly Button _save = new() { Name = "PresetSaveButton", Text = "Save" };
    private readonly Button _applyButton = new() { Name = "PresetApplyButton", Text = "Save / Apply" };
    private readonly Button _rename = new() { Name = "PresetRenameButton", Text = "Rename" };
    private readonly Button _delete = new() { Name = "PresetDeleteButton", Text = "Delete" };
    private readonly Button _reset = new() { Name = "PresetResetButton", Text = "Reset" };
    private ControlPreset _draft;
    private string _selectedId;
    private ControlAction _captureAction;
    private int _captureIndex;
    private Action? _confirmed;
    private bool _capturing;
    private bool _refreshing;

    public bool ModalOpen => _modal.Visible;

    public ControlPresetsPanel(ControlPresetStore store, Func<ControlPreset> activeProfile,
        Action<ControlPreset> apply, Action close)
    {
        _store = store;
        _activeProfile = activeProfile;
        _apply = apply;
        _close = close;
        _draft = store.Current;
        _selectedId = store.CurrentId;
        Name = "ControlPresetsPanel";
        Visible = false;
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _Ready()
    {
        var margin = new MarginContainer();
        foreach (var side in new[] { "left", "right", "top", "bottom" })
            margin.AddThemeConstantOverride("margin_" + side, 12);
        AddChild(margin);
        var body = new VBoxContainer();
        body.AddThemeConstantOverride("separation", 6);
        margin.AddChild(body);
        body.AddChild(new Label { Text = "Control Presets", HorizontalAlignment = HorizontalAlignment.Center });
        body.AddChild(new Label { Text = "Select a profile to edit. Apply changes the live controls." });
        body.AddChild(_profiles);
        _profiles.ItemSelected += index =>
        {
            if (_refreshing) return;
            var list = _store.List();
            if (index < 0 || index >= list.Count) return;
            Select(list[(int)index].Id);
        };
        body.AddChild(_state);
        var names = Row(body);
        AddButton(names, "Create", "PresetCreateButton", () => NameDialog("Create profile", "New Profile", name =>
        {
            var created = _store.Create(name);
            _store.Save();
            Select(created.Id);
        }));
        AddButton(names, "Save As", "PresetSaveAsButton", () => NameDialog("Save draft as", _draft.Name + " Copy", name =>
        {
            var created = _store.Duplicate(_selectedId, name);
            _store.Update(new ControlPreset(created.Id, name, Enum.GetValues<ControlAction>().ToDictionary(a => a,
                a => _draft.Bindings(a).ToArray()), _draft.MouseX, _draft.MouseY, _draft.MouseSensitivity));
            _store.Save();
            Select(created.Id);
        }));
        AddButton(names, "Duplicate", "PresetDuplicateButton", () => NameDialog("Duplicate profile", _draft.Name + " Copy", name =>
        {
            Select(_store.Duplicate(_selectedId, name).Id);
            _store.Save();
        }));
        names.AddChild(_rename);
        _rename.Pressed += () => NameDialog("Rename profile", _draft.Name, name =>
        {
            _store.Rename(_selectedId, name);
            _draft = _draft.WithName(name);
            _store.Save();
            Refresh();
        });
        names.AddChild(_delete);
        _delete.Pressed += () => Confirm("Delete this custom profile?", () =>
        {
            var wasActive = _store.CurrentId == _selectedId;
            _store.Delete(_selectedId);
            _store.Save();
            if (wasActive) _apply(_store.Current);
            Select(_store.CurrentId);
        });
        names.AddChild(_reset);
        _reset.Pressed += () => Confirm("Reset this custom profile to its template?", () =>
        {
            _store.Reset(_selectedId);
            _store.Save();
            Select(_selectedId);
            if (_store.CurrentId == _selectedId) _apply(_store.Current);
        });

        var scroll = new ScrollContainer { Name = "PresetActionsScroll", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, CustomMinimumSize = new Vector2(0, 170) };
        body.AddChild(scroll);
        _bindings.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(_bindings);
        body.AddChild(new Label { Text = "Mouse steering (movement is never captured as a button)" });
        var axis = Row(body);
        axis.AddChild(new Label { Text = "X" });
        axis.AddChild(_mouseX);
        axis.AddChild(_invertX);
        axis.AddChild(new Label { Text = "Y" });
        axis.AddChild(_mouseY);
        axis.AddChild(_invertY);
        foreach (var option in new[] { _mouseX, _mouseY })
        {
            option.AddItem("Off", (int)SteeringAxis.None);
            option.AddItem("Yaw", (int)SteeringAxis.Yaw);
            option.AddItem("Pitch", (int)SteeringAxis.Pitch);
            option.ItemSelected += _ => ChangeMouse();
        }
        _invertX.Toggled += _ => ChangeMouse();
        _invertY.Toggled += _ => ChangeMouse();
        var sensitivityRow = Row(body);
        sensitivityRow.AddChild(_sensitivityText);
        _sensitivity.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        sensitivityRow.AddChild(_sensitivity);
        _sensitivity.ValueChanged += _ => ChangeMouse();
        var actions = Row(body);
        actions.AddChild(_save);
        _save.Pressed += () => SaveDraft();
        actions.AddChild(_applyButton);
        _applyButton.Pressed += ApplyDraft;
        AddButton(actions, "Revert edits", "PresetRevertButton", () => Confirm("Discard unsaved edits and return to the active profile?", () => Select(_store.CurrentId)));
        actions.AddChild(_back);
        _back.Pressed += () =>
        {
            if (Dirty) Confirm("Discard unsaved edits and return to Options?", Close);
            else Close();
        };

        AddChild(_modal);
        _modal.SetAnchorsPreset(LayoutPreset.FullRect);
        var shade = new ColorRect { Color = new Color(0, 0, 0, 0.8f), MouseFilter = MouseFilterEnum.Stop };
        _modal.AddChild(shade);
        shade.SetAnchorsPreset(LayoutPreset.FullRect);
        var center = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
        _modal.AddChild(center);
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        var box = new PanelContainer { CustomMinimumSize = new Vector2(310, 0) };
        center.AddChild(box);
        var dialog = new VBoxContainer();
        box.AddChild(dialog);
        dialog.AddChild(_modalText);
        dialog.AddChild(_name);
        var dialogButtons = Row(dialog);
        dialogButtons.AddChild(_confirm);
        dialogButtons.AddChild(_cancel);
        _confirm.Pressed += ExecuteConfirm;
        _cancel.Pressed += CancelModal;
        _name.TextSubmitted += _ => ExecuteConfirm();
        Refresh();
    }

    private bool Dirty => !_draft.Name.Equals(_store.List().First(p => p.Id == _selectedId).Name, StringComparison.Ordinal) ||
        Enum.GetValues<ControlAction>().Any(a => !_draft.Bindings(a).SequenceEqual(_store.List().First(p => p.Id == _selectedId).Bindings(a))) ||
        _draft.MouseX != _store.List().First(p => p.Id == _selectedId).MouseX ||
        _draft.MouseY != _store.List().First(p => p.Id == _selectedId).MouseY ||
        _draft.MouseSensitivity != _store.List().First(p => p.Id == _selectedId).MouseSensitivity;

    private static HBoxContainer Row(Node parent)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 5);
        parent.AddChild(row);
        return row;
    }
    private static Button AddButton(Node parent, string text, string name, Action pressed)
    {
        var button = new Button { Name = name, Text = text, FocusMode = FocusModeEnum.All };
        parent.AddChild(button);
        button.Pressed += pressed;
        return button;
    }

    public void Open()
    {
        Visible = true;
        Select(_store.CurrentId);
        _profiles.GrabFocus();
    }
    private void Close()
    {
        CancelModal();
        Visible = false;
        _close();
    }
    private void Select(string id)
    {
        _selectedId = id;
        _draft = _store.List().First(p => p.Id == id);
        Refresh();
    }
    public void Refresh()
    {
        if (!IsNodeReady()) return;
        _refreshing = true;
        var profiles = _store.List();
        _profiles.Clear();
        for (var i = 0; i < profiles.Count; i++)
        {
            var profile = profiles[i];
            _profiles.AddItem(profile.Name + (profile.Id == _store.CurrentId ? " [Active]" : ""), i);
            if (profile.Id == _selectedId) _profiles.Select(i);
        }
        var active = _activeProfile();
        _state.Text = $"Editing: {_draft.Name}{(_draft.IsBuiltin ? " (built-in: duplicate to edit)" : " (custom)")} | Active: {active.Name}" +
            (Dirty ? " | Unsaved edits" : "") +
            (!Dirty && (_selectedId != active.Id || !SameControls(_draft, active)) ? " | Pending Apply" : "");
        _rename.Disabled = _delete.Disabled = _reset.Disabled = _draft.IsBuiltin;
        _save.Disabled = _draft.IsBuiltin;
        foreach (var child in _bindings.GetChildren()) child.QueueFree();
        foreach (var action in Enum.GetValues<ControlAction>())
        {
            var row = Row(_bindings);
            row.AddChild(new Label { Text = ActionName(action), CustomMinimumSize = new Vector2(115, 0) });
            var entries = _draft.Bindings(action);
            if (entries.Count < ControlPreset.MaxBindingsPerAction)
                AddButton(row, "+ binding", $"Add_{action}", () => BeginCapture(action, -1)).Disabled = _draft.IsBuiltin;
            for (var i = 0; i < entries.Count; i++)
            {
                var index = i;
                var bindingRow = Row(_bindings);
                bindingRow.AddChild(new Label { Text = "   " });
                AddButton(bindingRow, BindingName(entries[i]), $"Binding_{action}_{i}", () => BeginCapture(action, index)).Disabled = _draft.IsBuiltin;
                AddButton(bindingRow, "Remove", $"Remove_{action}_{i}", () => Remove(action, index)).Disabled = _draft.IsBuiltin;
            }
        }
        _mouseX.Select((int)_draft.MouseX.Steering);
        _mouseY.Select((int)_draft.MouseY.Steering);
        _invertX.ButtonPressed = _draft.MouseX.Inverted;
        _invertY.ButtonPressed = _draft.MouseY.Inverted;
        _sensitivity.Value = _draft.MouseSensitivity;
        _sensitivityText.Text = $"Sensitivity: {_draft.MouseSensitivity:F2}°/px";
        _mouseX.Disabled = _mouseY.Disabled = _invertX.Disabled = _invertY.Disabled = _draft.IsBuiltin;
        _sensitivity.Editable = !_draft.IsBuiltin;
        _refreshing = false;
    }

    private void ChangeMouse()
    {
        if (_refreshing || _draft.IsBuiltin) return;
        try
        {
            _draft = _draft.WithMouse(new((SteeringAxis)_mouseX.Selected, _invertX.ButtonPressed),
                new((SteeringAxis)_mouseY.Selected, _invertY.ButtonPressed), (float)_sensitivity.Value);
            Refresh();
        }
        catch (ArgumentException)
        {
            Refresh();
            _state.Text += " | X and Y cannot steer the same direction.";
        }
    }
    private void Remove(ControlAction action, int index)
    {
        var entries = _draft.Bindings(action).ToList();
        entries.RemoveAt(index);
        _draft = _draft.WithBindings(action, entries.ToArray());
        Refresh();
    }
    private void BeginCapture(ControlAction action, int index)
    {
        _captureAction = action;
        _captureIndex = index;
        _capturing = true;
        _modalText.Text = $"{ActionName(action)}: press a physical key or mouse button. Escape cancels. Pointer movement is ignored.";
        _name.Visible = _confirm.Visible = false;
        _modal.Visible = true;
        _cancel.GrabFocus();
    }
    public bool HandleModalInput(InputEvent input)
    {
        if (!_modal.Visible) return false;
        if (input is InputEventKey { Pressed: true, Echo: false } key && key.PhysicalKeycode == Key.Escape)
        {
            CancelModal();
            return true;
        }
        if (!_capturing) return false;
        if (input is InputEventKey { Pressed: true, Echo: false } physical && physical.PhysicalKeycode != Key.None)
            Capture(new(BindingDevice.PhysicalKey, (int)physical.PhysicalKeycode));
        else if (input is InputEventMouseButton cancelClick && _cancel.GetGlobalRect().HasPoint(cancelClick.Position))
            return false;
        else if (input is InputEventMouseButton { Pressed: true } button && button.ButtonIndex is
                 MouseButton.Left or MouseButton.Right or MouseButton.Middle or MouseButton.Xbutton1 or MouseButton.Xbutton2)
            Capture(new(BindingDevice.MouseButton, (int)button.ButtonIndex));
        return true;
    }
    private void Capture(ControlBinding binding)
    {
        foreach (var action in Enum.GetValues<ControlAction>())
        {
            if (!_draft.Bindings(action).Contains(binding)) continue;
            if (action != _captureAction || _captureIndex < 0 || _draft.Bindings(action)[_captureIndex] != binding)
            {
                _modalText.Text = $"{BindingName(binding)} is already assigned to {ActionName(action)}. Remove it there first, then retry. Escape cancels.";
                return;
            }
        }
        try
        {
            var entries = _draft.Bindings(_captureAction).ToList();
            if (_captureIndex < 0) entries.Add(binding);
            else entries[_captureIndex] = binding;
            _draft = _draft.WithBindings(_captureAction, entries.ToArray());
            CancelModal();
            Refresh();
        }
        catch (ArgumentException error)
        {
            _modalText.Text = $"Cannot use {BindingName(binding)}: {error.Message}. Escape cancels.";
        }
    }
    private void NameDialog(string title, string suggestion, Action<string> action)
    {
        _capturing = false;
        _modalText.Text = title;
        _name.Text = suggestion;
        _name.Visible = _confirm.Visible = true;
        _confirmed = () => action(_name.Text.Trim());
        _modal.Visible = true;
        _name.GrabFocus();
        _name.SelectAll();
    }
    private void Confirm(string message, Action action)
    {
        _capturing = false;
        _modalText.Text = message;
        _name.Visible = false;
        _confirm.Visible = true;
        _confirmed = action;
        _modal.Visible = true;
        _cancel.GrabFocus();
    }
    private void ExecuteConfirm()
    {
        try
        {
            _confirmed?.Invoke();
            CancelModal();
            Refresh();
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or System.IO.IOException or UnauthorizedAccessException)
        {
            _modalText.Text = error.Message;
        }
    }
    public void CancelModal()
    {
        _capturing = false;
        _confirmed = null;
        _modal.Visible = false;
        if (Visible && IsNodeReady()) _profiles.GrabFocus();
    }
    public void RequestClose()
    {
        if (ModalOpen) { CancelModal(); return; }
        if (Dirty) Confirm("Discard unsaved edits and return to Options?", Close);
        else Close();
    }
    private bool SaveDraft()
    {
        if (_draft.IsBuiltin) { _state.Text += " | Duplicate this built-in first."; return false; }
        var previous = _store.List().First(p => p.Id == _selectedId);
        try { _store.Update(_draft); _store.Save(); Refresh(); return true; }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or System.IO.IOException or UnauthorizedAccessException)
        {
            _store.Update(previous);
            _state.Text += " | Save failed: " + error.Message;
            return false;
        }
    }
    private void ApplyDraft()
    {
        if (!_draft.IsBuiltin && !SaveDraft()) return;
        var previousId = _store.CurrentId;
        try
        {
            _store.Choose(_selectedId);
            _store.Save();
            _apply(_store.Current);
            Refresh();
        }
        catch (Exception error) when (error is ArgumentException or System.IO.IOException or UnauthorizedAccessException)
        {
            _store.Choose(previousId);
            _state.Text += " | Apply failed: " + error.Message;
        }
    }
    public static string ActionName(ControlAction action) => action switch
    {
        ControlAction.Thrust => "Swim / thrust", ControlAction.Slow => "Slow / brake",
        ControlAction.YawLeft => "Turn left", ControlAction.YawRight => "Turn right",
        ControlAction.PitchUp => "Pitch up", ControlAction.PitchDown => "Pitch down",
        _ => "Pause / Options"
    };
    public static string BindingName(ControlBinding binding) => binding.Device == BindingDevice.PhysicalKey
        ? ((Key)binding.Code).ToString() : ((MouseButton)binding.Code) switch
        {
            MouseButton.Left => "Left mouse", MouseButton.Right => "Right mouse",
            MouseButton.Middle => "Middle mouse", MouseButton.Xbutton1 => "Mouse 4",
            MouseButton.Xbutton2 => "Mouse 5", _ => "Mouse " + binding.Code
        };

    private static bool SameControls(ControlPreset a, ControlPreset b) =>
        a.MouseX == b.MouseX && a.MouseY == b.MouseY && a.MouseSensitivity == b.MouseSensitivity &&
        Enum.GetValues<ControlAction>().All(action => a.Bindings(action).SequenceEqual(b.Bindings(action)));
}
