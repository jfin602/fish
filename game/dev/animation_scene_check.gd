extends SceneTree

# Real imported Fish3 actor, motor and camera. Presentation must stay below the collision root.
func _initialize() -> void:
	call_deferred("_run")

func _require(ok: bool, message: String) -> bool:
	if ok:
		return true
	printerr("ANIMATION_SCENE_FAILED ", message)
	quit(1)
	return false

func _tick(count: int) -> void:
	for i in range(count):
		await physics_frame

func _key(code: Key, down: bool) -> void:
	var event := InputEventKey.new()
	event.keycode = code
	event.physical_keycode = code
	event.pressed = down
	Input.parse_input_event(event)

func _state(motion: Node) -> Vector3:
	return motion.call("GetPresentationState")

func _run() -> void:
	var scene: Node3D = load("res://scenes/p1_course_fixture.tscn").instantiate()
	root.add_child(scene)
	current_scene = scene
	var fish: CharacterBody3D = scene.get_node("Fish")
	var visual: Node3D = scene.get_node("Fish/PlayerVisual")
	var motion: Node = scene.get_node("Fish/PlayerVisual/SwimPresentation")
	var skeleton: Skeleton3D = scene.get_node("Fish/PlayerVisual/Armature/Skeleton3D")
	var player: AnimationPlayer = scene.get_node("Fish/PlayerVisual/AnimationPlayer")
	var camera: Camera3D = scene.get_node("Fish/CameraRig/Camera3D")
	var collider: CollisionShape3D = scene.get_node("Fish/Collision")
	var base_visual_rotation := visual.quaternion
	var clip: Animation = player.get_animation("Armature|Swim")
	if not _require(clip.length > 1.28 and clip.length < 1.30 and
			clip.loop_mode == Animation.LOOP_NONE and clip.get_track_count() == 3 and
			skeleton.get_bone_count() == 8 and player.current_animation == "Armature|Swim",
			"imported Fish3 clip, rig or playback mapping changed"):
		return
	for name in ["Root", "Spine3", "Tail"]:
		var path := NodePath("Armature/Skeleton3D:" + name)
		var track := clip.find_track(path, Animation.TYPE_ROTATION_3D)
		if not _require(skeleton.find_bone(name) >= 0 and track >= 0 and
				clip.track_get_key_count(track) >= 2,
				"required deforming bone or rotation track missing: " + name):
			return
	var tail := skeleton.find_bone("Tail")
	var face := skeleton.find_bone("Face")
	var rest_facing := (skeleton.to_global(skeleton.get_bone_global_rest(face).origin) -
		skeleton.to_global(skeleton.get_bone_global_rest(tail).origin)).normalized()
	if not _require(rest_facing.dot(-fish.global_basis.z) > 0.85,
			"Fish3 nose does not align with the authoritative swim heading"):
		return
	var neutral := clip.rotation_track_interpolate(
		clip.find_track(NodePath("Armature/Skeleton3D:Tail"), Animation.TYPE_ROTATION_3D), 0.0)
	# Give the motor room to coast without meeting the north course boundary.
	fish.position = Vector3(0, 0, 4)
	await _tick(3)
	_key(KEY_W, true)
	await _tick(24)
	var powered := _state(motion)
	if not _require(powered.x > 0.8 and fish.velocity.length() > 0.5 and
			skeleton.get_bone_pose_rotation(tail).angle_to(neutral) > 0.05,
			"propulsion did not increase swim pose and authoritative speed: activity=%.3f speed=%.3f tail=%.3f position=%s" % [powered.x, fish.velocity.length(), skeleton.get_bone_pose_rotation(tail).angle_to(neutral), fish.position]):
		return
	var previous_phase := powered.z
	var previous_tail := skeleton.get_bone_pose_rotation(tail)
	var wraps := 0
	var seam_jump := 0.0
	for i in range(90):
		await _tick(1)
		var phase := _state(motion).z
		var pose := skeleton.get_bone_pose_rotation(tail)
		if phase < previous_phase:
			wraps += 1
			seam_jump = maxf(seam_jump, pose.angle_to(previous_tail))
		previous_phase = phase
		previous_tail = pose
	if not _require(wraps >= 1 and seam_jump < 0.2 and
			player.current_animation_position < clip.length,
			"swim phase did not loop continuously across imported clip seam"):
		return
	_key(KEY_W, false)
	var release_phase := _state(motion).z
	await _tick(30)
	var gliding := _state(motion)
	if not _require(gliding.x < powered.x * 0.55 and gliding.x > 0.05 and
			fish.velocity.length() > 0.5 and gliding.z != release_phase,
			"release failed to ease activity while momentum and glide animation continue"):
		return
	_key(KEY_D, true)
	await _tick(20)
	var right_turn := _state(motion)
	var right_pose := visual.quaternion
	if not _require(right_turn.y > 0.7 and right_pose.angle_to(base_visual_rotation) > 0.04 and
			absf(camera.global_basis.x.y) < 0.001,
			"right turn lacks visual bend/lean or rolls camera"):
		return
	_key(KEY_D, false)
	_key(KEY_A, true)
	await _tick(24)
	var left_turn := _state(motion)
	if not _require(left_turn.y < -0.7 and visual.quaternion.angle_to(right_pose) > 0.12 and
			absf(camera.global_basis.x.y) < 0.001,
			"opposite turn did not reverse visual pose with steady horizon"):
		return
	_key(KEY_A, false)
	await _tick(70)
	if not _require(absf(_state(motion).y) < 0.01 and
			visual.quaternion.angle_to(base_visual_rotation) < 0.01,
			"released turn did not restore base visual orientation"):
		return
	paused = true
	var paused_phase := _state(motion).z
	var paused_pose := skeleton.get_bone_pose_rotation(tail)
	var paused_eye := camera.global_transform
	await create_timer(0.15).timeout
	if not _require(absf(_state(motion).z - paused_phase) < 0.00001 and
			skeleton.get_bone_pose_rotation(tail).angle_to(paused_pose) < 0.00001 and
			camera.global_transform.is_equal_approx(paused_eye),
			"pause advanced animation or camera"):
		return
	paused = false
	await _tick(2)
	fish.set_physics_process(false)
	var root_before := fish.global_transform
	var collider_before := collider.global_transform
	await _tick(20)
	if not _require(fish.global_transform.is_equal_approx(root_before) and
			collider.global_transform.is_equal_approx(collider_before) and
			absf(camera.global_basis.x.y) < 0.001,
			"presentation moved authoritative fish/collider or banked camera"):
		return
	print("ANIMATION_SCENE_OK activity=%.3f glide=%.3f wraps=%d seam=%.3f right=%.3f left=%.3f" % [
		powered.x, gliding.x, wraps, seam_jump, right_turn.y, left_turn.y])
	quit()
