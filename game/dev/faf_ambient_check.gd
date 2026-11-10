extends SceneTree

const EXPECTED := {"shallow": 13, "upper": 8, "middle": 6, "lower": 2, "deep": 2}
const IDS := ["FAF-FISH-001", "FAF-FISH-002", "FAF-FISH-003", "FAF-FISH-004"]

func _initialize() -> void:
	call_deferred("_run")

func fail(message: String) -> void:
	printerr("FAF_AMBIENT_FAILED ", message)
	quit(1)

func contains_gameplay(node: Node) -> bool:
	if node is CollisionObject3D or node is CollisionShape3D or node is CharacterBody3D or node is Area3D:
		return true
	if node.get_script() != null:
		return true
	for child in node.get_children():
		if contains_gameplay(child):
			return true
	return false

func _run() -> void:
	var main: Node3D = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	for index in range(3):
		await physics_frame
	var reef: Node3D = main.get_node("Habitat/Composition/ReefComposition")
	var ambient: Node3D = reef.get_node("AmbientLife")
	var visuals: Node3D = ambient.get_node("Visuals")
	var seabed: Node3D = main.get_node("Habitat/Seabed")
	var player: CharacterBody3D = main.get_node("Fish")
	var player_visual: Node3D = player.get_node("PlayerVisual")
	if ambient.SchoolCount != 9 or ambient.FishCount != 31 or visuals.get_child_count() != 31:
		fail("population is not bounded to nine schools and 31 visuals")
		return
	var counts := {"shallow": 0, "upper": 0, "middle": 0, "lower": 0, "deep": 0}
	var ids := {}
	var clips := {}
	var first_pose: Transform3D = ambient.GetPose(0, 0.0, seabed)
	for index in range(visuals.get_child_count()):
		var actor: Node3D = visuals.get_child(index)
		var id: String = actor.get_meta("faf_id", "")
		var zone: String = actor.get_meta("zone", "")
		var rig: Node3D = actor.get_node("Rig")
		var animation: AnimationPlayer = rig.find_children("*", "AnimationPlayer", true, false)[0]
		if not IDS.has(id) or not counts.has(zone) or actor.get_meta("school", "") == "":
			fail("unknown catalog ID, depth or school")
			return
		if contains_gameplay(actor) or actor.is_ancestor_of(player_visual) or player_visual.is_ancestor_of(actor):
			fail("ambient visual owns gameplay state or player art")
			return
		if not animation.is_playing() or not animation.current_animation.ends_with("__Glide") and not animation.current_animation.ends_with("__Swim") and not animation.current_animation.ends_with("__SwimFast"):
			fail("missing imported looping presentation animation on " + id)
			return
		var initial: Transform3D = ambient.GetPose(index, 0.0, seabed)
		var replay: Transform3D = ambient.GetPose(index, 0.0, seabed)
		var later: Transform3D = ambient.GetPose(index, 12.0, seabed)
		if not initial.is_equal_approx(replay) or initial.origin.distance_to(later.origin) < 0.05:
			fail("loop is nondeterministic or stationary")
			return
		if initial.origin.y < seabed.GetHeightAt(initial.origin.x, initial.origin.z) + 1.0 or initial.origin.y > 8.5:
			fail("loop leaves the bounded water pocket")
			return
		counts[zone] += 1
		ids[id] = true
		clips[animation.current_animation] = true
	if counts != EXPECTED or ids.size() != 4 or counts.shallow + counts.upper <= counts.lower + counts.deep:
		fail("depth design, four-fish coverage or shallow bias changed: " + str(counts))
		return
	if not first_pose.is_equal_approx(ambient.GetPose(0, 0.0, seabed)) or clips.size() < 8:
		fail("loop replay or clip diversity changed")
		return
	if player.get_node("Collision") == null or player_visual.scene_file_path == "" or player_visual.get_parent() != player:
		fail("selected/legacy player art lost independent ownership")
		return
	print("FAF_AMBIENT_OK schools=9 fish=31 zones=%s ids=4 clips=%d presentation_only=true" % [counts, clips.size()])
	quit()
