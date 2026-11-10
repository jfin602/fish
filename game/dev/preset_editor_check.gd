extends SceneTree

# Exercises the saved scene's C# pause editor with physical key and mouse events.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, reason: String) -> bool:
	if ok:
		return true
	printerr("PRESET_EDITOR_FAILED ", reason)
	quit(1)
	return false

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func _mouse(code: MouseButton, down: bool, point: Vector2 = Vector2.ZERO) -> void:
	var event := InputEventMouseButton.new()
	event.button_index = code
	event.pressed = down
	event.position = point
	event.global_position = point
	root.push_input(event, true)

func _click(control: Control) -> void:
	var point := control.get_global_rect().get_center()
	_mouse(MOUSE_BUTTON_LEFT, true, point)
	await process_frame
	_mouse(MOUSE_BUTTON_LEFT, false, point)
	await process_frame

func _button(shell: Node, name: String) -> Button:
	return shell.find_child(name, true, false) as Button

func _run() -> void:
	var packed: PackedScene = load("res://scenes/main.tscn")
	var scene: Node3D = packed.instantiate()
	root.add_child(scene)
	current_scene = scene
	await process_frame
	var shell: Node = scene.get_node("PlayShell")
	var fish: CharacterBody3D = scene.get_node("Fish")
	var options := _button(shell, "OptionsButton")
	var editor := shell.find_child("ControlPresetsPanel", true, false) as Control
	var choice := shell.find_child("PresetProfileOption", true, false) as OptionButton
	var state := shell.find_child("PresetStateLabel", true, false) as Label
	var name_edit := shell.find_child("PresetNameEdit", true, false) as LineEdit
	var confirm := _button(shell, "PresetConfirmButton")
	var cancel := _button(shell, "PresetCancelButton")
	var modal := shell.find_child("PresetModal", true, false) as Control
	var modal_text := shell.find_child("PresetModalText", true, false) as Label
	if not _require(editor != null and choice != null and options != null and confirm != null,
			"real editor controls absent"):
		return
	await _click(options)
	if not _require(paused and _button(shell, "ResumeButton").has_focus(), "pointer Options failed"):
		return
	await _click(_button(shell, "ControlPresetsButton"))
	if not _require(editor.visible and choice.has_focus() and choice.item_count == 5,
			"editor entry, focus or built-ins missing"):
		return
	var viewport_size := scene.get_viewport().get_visible_rect().size
	var editor_rect := editor.get_global_rect()
	if not _require(editor_rect.position.x >= -1 and editor_rect.position.y >= -1 and
			editor_rect.end.x <= viewport_size.x + 1 and editor_rect.end.y <= viewport_size.y + 1,
			"editor exceeds viewport %s / %s" % [editor_rect, viewport_size]):
		return
	root.size = Vector2i(640, 480)
	await process_frame
	viewport_size = scene.get_viewport().get_visible_rect().size
	editor_rect = editor.get_global_rect()
	print("PRESET_EDITOR_LAYOUT requested=640x480 viewport=", viewport_size, " panel=", editor_rect)
	if not _require(editor_rect.position.x >= -1 and editor_rect.position.y >= -1 and
			editor_rect.end.x <= viewport_size.x + 1 and editor_rect.end.y <= viewport_size.y + 1,
			"editor exceeds smaller viewport %s / %s" % [editor_rect, viewport_size]):
		return
	for control_name in ["PresetBackButton", "PresetApplyButton", "PresetCreateButton", "PresetDeleteButton"]:
		var control_rect := _button(shell, control_name).get_global_rect()
		if not _require(editor_rect.encloses(control_rect),
				"%s is outside the smaller editor panel: %s" % [control_name, control_rect]):
			return
	if not _require(_button(shell, "PresetSaveButton").disabled and
			_button(shell, "PresetDeleteButton").disabled, "built-in was editable"):
		return
	_key(KEY_TAB, true)
	await process_frame
	_key(KEY_TAB, false)
	if not _require(not choice.has_focus(), "Tab did not advance keyboard focus"):
		return
	_button(shell, "PresetCreateButton").pressed.emit()
	name_edit.text = "Current Tester"
	confirm.pressed.emit()
	await process_frame
	if not _require(choice.item_count == 6 and state.text.contains("Editing: Current Tester") and
			state.text.contains("Active: Keyboard Only"), "create changed active or did not show custom"):
		return
	_button(shell, "Add_Thrust").pressed.emit()
	_key(KEY_J, true)
	await process_frame
	_key(KEY_J, false)
	await process_frame
	if not _require(not modal.visible and _button(shell, "Binding_Thrust_1") != null and
			_button(shell, "Binding_Thrust_1").text == "J", "physical key capture failed"):
		return
	_button(shell, "Add_Thrust").pressed.emit()
	_mouse(MOUSE_BUTTON_XBUTTON1, true)
	await process_frame
	_mouse(MOUSE_BUTTON_XBUTTON1, false)
	await process_frame
	if not _require(_button(shell, "Binding_Thrust_2") != null and
			_button(shell, "Binding_Thrust_2").text == "Mouse 4", "mouse button capture failed"):
		return
	_button(shell, "Add_Thrust").pressed.emit()
	_key(KEY_S, true)
	await process_frame
	_key(KEY_S, false)
	if not _require(modal.visible and modal_text.text.contains("already assigned to Slow"),
			"conflict was not explained"):
		return
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	if not _require(editor.visible and not modal.visible and paused,
			"Escape failed to cancel capture safely"):
		return
	_button(shell, "Add_Thrust").pressed.emit()
	await _click(cancel)
	if not _require(not modal.visible and _button(shell, "Binding_Thrust_3") == null,
			"pointer Cancel captured the click as a binding"):
		return
	_button(shell, "Remove_Thrust_2").pressed.emit()
	await process_frame
	_button(shell, "Binding_Thrust_1").pressed.emit()
	_key(KEY_K, true)
	await process_frame
	_key(KEY_K, false)
	await process_frame
	if not _require(_button(shell, "Binding_Thrust_1").text == "K", "replace/remove failed"):
		return
	var mouse_x := shell.find_child("MouseXAxisOption", true, false) as OptionButton
	var mouse_y := shell.find_child("MouseYAxisOption", true, false) as OptionButton
	mouse_x.select(1)
	mouse_x.item_selected.emit(1)
	mouse_y.select(1)
	mouse_y.item_selected.emit(1)
	if not _require(state.text.contains("cannot steer the same direction") and mouse_y.selected == 0,
			"mouse axis conflict was not rejected"):
		return
	mouse_y.select(2)
	mouse_y.item_selected.emit(2)
	(shell.find_child("InvertMouseYEditorToggle", true, false) as CheckButton).button_pressed = true
	var sensitivity := shell.find_child("PresetSensitivitySlider", true, false) as HSlider
	sensitivity.value = 0.19
	_button(shell, "PresetRevertButton").pressed.emit()
	if not _require(modal.visible, "revert lacked confirmation"):
		return
	cancel.pressed.emit()
	if not _require(state.text.contains("Unsaved edits"), "cancel lost draft"):
		return
	_button(shell, "PresetSaveButton").pressed.emit()
	var help := scene.get_node("PrototypeHelp/Help") as Label
	if not _require(state.text.contains("Pending Apply") and not help.text.contains("Current Tester"),
			"Save changed the active help before Apply"):
		return
	_button(shell, "PresetApplyButton").pressed.emit()
	var active_id: String = fish.call("GetActivePresetId")
	if not _require(active_id.begins_with("custom:") and state.text.contains("Active: Current Tester"),
			"save/apply failed"):
		return
	if not _require(help.text.contains("K") and help.text.contains("Current Tester"),
			"show-controls help did not follow applied bindings"):
		return
	_button(shell, "PresetRenameButton").pressed.emit()
	name_edit.text = "Renamed Tester"
	confirm.pressed.emit()
	if not _require(state.text.contains("Renamed Tester"), "rename failed"):
		return
	_button(shell, "PresetDuplicateButton").pressed.emit()
	name_edit.text = "Copy Tester"
	confirm.pressed.emit()
	if not _require(choice.item_count == 7, "duplicate failed"):
		return
	_button(shell, "PresetDeleteButton").pressed.emit()
	if not _require(modal.visible, "delete lacked confirmation"):
		return
	confirm.pressed.emit()
	if not _require(choice.item_count == 6 and fish.call("GetActivePresetId") == active_id,
			"delete changed active profile"):
		return
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	if not _require(not editor.visible and paused, "Escape did not return to Options"):
		return
	_key(KEY_K, true)
	await _click(_button(shell, "ResumeButton"))
	await physics_frame
	if not _require(not paused and fish.call("GetLastThrust") == 0.0,
			"held captured key or Resume activation leaked thrust on first gameplay frame"):
		return
	_key(KEY_K, false)
	await physics_frame
	await physics_frame
	_key(KEY_K, true)
	await physics_frame
	await physics_frame
	if not _require(fish.call("GetLastThrust") == 1.0,
			"fresh press after Resume did not rearm: armed=%s held=%s thrust=%s" % [fish.call("GetInputArmed"), fish.call("GetHeldInputCount"), fish.call("GetLastThrust")]):
		return
	_key(KEY_K, false)
	var saved := FileAccess.open("user://control-presets.json", FileAccess.READ)
	if not _require(saved != null and saved.get_as_text().contains("Renamed Tester"),
			"custom profile was not persisted"):
		return
	scene.queue_free()
	await process_frame
	var reopened: Node3D = packed.instantiate()
	root.add_child(reopened)
	current_scene = reopened
	await process_frame
	var second_shell: Node = reopened.get_node("PlayShell")
	var second_fish: CharacterBody3D = reopened.get_node("Fish")
	if not _require(second_fish.call("GetActivePresetId") == active_id and
			(second_shell.find_child("InputModeOption", true, false) as OptionButton).item_count == 6,
			"relaunch did not recover active custom profile"):
		return
	await _click(_button(second_shell, "OptionsButton"))
	await _click(_button(second_shell, "ControlPresetsButton"))
	var second_editor := second_shell.find_child("ControlPresetsPanel", true, false) as Control
	var second_slider := second_shell.find_child("PresetSensitivitySlider", true, false) as HSlider
	if not _require(second_editor.visible and absf(second_slider.value - 0.19) < 0.001 and
			(second_shell.find_child("InvertMouseYEditorToggle", true, false) as CheckButton).button_pressed,
			"mouse settings did not reload"):
		return
	second_slider.value = 0.27
	_button(second_shell, "PresetRevertButton").pressed.emit()
	_button(second_shell, "PresetConfirmButton").pressed.emit()
	if not _require(absf(second_slider.value - 0.19) < 0.001 and
			second_fish.call("GetActivePresetId") == active_id, "confirmed revert changed active"):
		return
	_button(second_shell, "PresetSaveAsButton").pressed.emit()
	(second_shell.find_child("PresetNameEdit", true, false) as LineEdit).text = "Saved As Tester"
	_button(second_shell, "PresetConfirmButton").pressed.emit()
	if not _require((second_shell.find_child("PresetProfileOption", true, false) as OptionButton).item_count == 7 and
			_button(second_shell, "Binding_Thrust_1").text == "K", "Save As lost draft"):
		return
	_button(second_shell, "PresetDeleteButton").pressed.emit()
	_button(second_shell, "PresetConfirmButton").pressed.emit()
	_button(second_shell, "PresetResetButton").pressed.emit()
	_button(second_shell, "PresetConfirmButton").pressed.emit()
	await process_frame
	if not _require(_button(second_shell, "Binding_Thrust_1") == null and
			second_fish.call("GetActivePresetId") == active_id, "reset did not restore template safely"):
		return
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	var preset_entry := _button(second_shell, "ControlPresetsButton")
	for i in 12:
		if preset_entry.has_focus():
			break
		_key(KEY_TAB, true)
		await process_frame
		_key(KEY_TAB, false)
		await process_frame
	if not _require(preset_entry.has_focus(), "keyboard Tab could not reach Control Presets"):
		return
	_key(KEY_ENTER, true)
	await process_frame
	_key(KEY_ENTER, false)
	await process_frame
	if not _require(second_editor.visible, "keyboard Enter did not open Control Presets"):
		return
	_key(KEY_TAB, true)
	await process_frame
	_key(KEY_TAB, false)
	await process_frame
	if not _require(_button(second_shell, "PresetCreateButton").has_focus(),
			"keyboard Tab did not reach Create"):
		return
	_key(KEY_ENTER, true)
	await process_frame
	_key(KEY_ENTER, false)
	await process_frame
	if not _require((second_shell.find_child("PresetNameEdit", true, false) as LineEdit).has_focus(),
			"keyboard Enter did not open focused name editor"):
		return
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	if not _require(not second_editor.visible and paused, "keyboard Escape did not leave editor"):
		return
	await _click(preset_entry)
	_button(second_shell, "PresetDeleteButton").pressed.emit()
	_button(second_shell, "PresetConfirmButton").pressed.emit()
	if not _require(second_fish.call("GetActivePresetId") == "builtin:keyboard" and
			(second_shell.find_child("PresetProfileOption", true, false) as OptionButton).item_count == 5,
			"deleting active custom did not fall back safely"):
		return
	_key(KEY_ESCAPE, true)
	await process_frame
	_key(KEY_ESCAPE, false)
	var legacy_mode := second_shell.find_child("InputModeOption", true, false) as OptionButton
	legacy_mode.select(1)
	legacy_mode.item_selected.emit(1)
	if not _require(second_fish.call("GetActivePresetId") == "builtin:mouse", "mouse-only mode unavailable"):
		return
	await _click(preset_entry)
	if not _require(second_editor.visible, "mouse-only pointer could not open editor"):
		return
	await _click(_button(second_shell, "PresetBackButton"))
	if not _require(not second_editor.visible and paused, "mouse-only pointer could not leave editor"):
		return
	print("PRESET_EDITOR_OK create save-as duplicate rename delete reset capture conflict cancel revert apply reload focus pointer resume")
	quit()
