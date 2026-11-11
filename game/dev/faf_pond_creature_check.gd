extends SceneTree

# Focused import/rig/clip proof for three independent Pond GLBs.
const MANIFEST := "res://../assets/faf/pond/creatures/pond_creature_001_manifest.json"


func _initialize() -> void:
	var path := ProjectSettings.globalize_path(MANIFEST)
	var data: Variant = JSON.parse_string(FileAccess.get_file_as_string(path))
	if not data is Dictionary or not data.has("assets") or data["assets"].size() != 3:
		_fail("manifest missing three assets")
		return
	for item in data["assets"]:
		var entry: Dictionary = item
		var stem: String = entry["stem"]
		var packed := load("res://assets/faf/pond/creatures/" + stem + ".glb") as PackedScene
		if packed == null:
			_fail(stem + " GLB not imported")
			return
		var model := packed.instantiate() as Node3D
		root.add_child(model)
		if model.position != Vector3.ZERO or model.rotation != Vector3.ZERO or model.scale != Vector3.ONE:
			_fail(stem + " nonidentity root")
			return
		var skeletons: Array[Skeleton3D] = []
		var players: Array[AnimationPlayer] = []
		var meshes: Array[MeshInstance3D] = []
		_collect(model, skeletons, players, meshes)
		if skeletons.size() != 1 or players.size() != 1 or meshes.is_empty():
			_fail(stem + " missing skeleton/player/mesh")
			return
		var skeleton := skeletons[0]
		var player := players[0]
		if skeleton.get_bone_count() != int(entry["bones"]):
			_fail(stem + " bone count mismatch")
			return
		for bone_name in entry["bone_names"]:
			if skeleton.find_bone(String(bone_name)) < 0:
				_fail(stem + " missing bone " + String(bone_name))
				return
		var tri_count := 0
		var mat_names := {}
		var bounds := AABB()
		var first := true
		var skinned_meshes := 0
		for mesh in meshes:
			bounds = mesh.get_aabb() if first else bounds.merge(mesh.get_aabb())
			first = false
			if mesh.skin != null and mesh.skeleton != NodePath(""):
				skinned_meshes += 1
			for surface_index in mesh.mesh.get_surface_count():
				tri_count += mesh.mesh.surface_get_array_index_len(surface_index) / 3
				var material := mesh.mesh.surface_get_material(surface_index)
				if material != null:
					mat_names[material.resource_name] = true
		if skinned_meshes != meshes.size() or tri_count != int(entry["triangles"]) or mat_names.size() != int(entry["materials"]):
			_fail(stem + " geometry/skin/material count mismatch " + str([tri_count,mat_names.size(),skinned_meshes,meshes.size()]))
			return
		var dimensions: Array = entry["dimensions_m"]
		if absf(bounds.size.x - float(dimensions[0])) > .0002 or absf(bounds.size.y - float(dimensions[2])) > .0002 or absf(bounds.size.z - float(dimensions[1])) > .0002:
			_fail(stem + " dimensions differ from Blender source")
			return
		for action_name in entry["actions"]:
			var clip: String = action_name
			if not player.get_animation_list().has(clip):
				_fail(stem + " missing " + clip)
				return
			var action := player.get_animation(clip)
			var role := clip.trim_prefix(stem + "__")
			var looping: bool = entry["loop_roles"].has(role)
			if action.length <= 0 or action.loop_mode != (Animation.LOOP_LINEAR if looping else Animation.LOOP_NONE):
				_fail(stem + " wrong clip length/loop " + role)
				return
			if action.get_track_count() == 0:
				_fail(stem + " empty clip " + role)
				return
			player.play(clip)
			player.seek(0, true)
			player.advance(0)
			var root_before := skeleton.get_bone_pose(skeleton.find_bone("root"))
			var moving_name := "hind_lower_L" if stem.contains("frog") else "spine_08" if stem.contains("snake") else "fore_upper_L"
			var bone_index := skeleton.find_bone(moving_name)
			var moving_before := skeleton.get_bone_pose(bone_index)
			if looping:
				player.seek(action.length * .35, true)
				player.advance(0)
			else:
				player.advance(action.length * .65)
			if not root_before.is_equal_approx(skeleton.get_bone_pose(skeleton.find_bone("root"))):
				_fail(stem + " gameplay root moved in " + role)
				return
			if moving_before.is_equal_approx(skeleton.get_bone_pose(bone_index)):
				_fail(stem + " no articulated movement in " + role)
				return
			if looping:
				player.seek(0, true)
				player.advance(0)
				var start_pose := skeleton.get_bone_pose(bone_index)
				player.seek(action.length, true)
				player.advance(0)
				if not start_pose.is_equal_approx(skeleton.get_bone_pose(bone_index)):
					_fail(stem + " loop seam mismatch " + role)
					return
			else:
				player.advance(action.length)
				if player.is_playing():
					_fail(stem + " one-shot did not stop " + role)
					return
		print("FAF_POND_CREATURE_IMPORTED ", stem, " triangles=", tri_count,
			" materials=", mat_names.size(), " bones=", skeleton.get_bone_count(),
			" actions=", entry["actions"].size(), " skinned_meshes=", skinned_meshes)
		model.queue_free()
	print("FAF_POND_CREATURE_GODOT_OK count=3")
	quit(0)


func _collect(node: Node, skeletons: Array[Skeleton3D], players: Array[AnimationPlayer], meshes: Array[MeshInstance3D]) -> void:
	if node is Skeleton3D:
		skeletons.append(node as Skeleton3D)
	if node is AnimationPlayer:
		players.append(node as AnimationPlayer)
	if node is MeshInstance3D:
		meshes.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect(child,skeletons,players,meshes)


func _fail(message: String) -> void:
	printerr("FAF_POND_CREATURE_GODOT_FAILED ",message)
	quit(1)
