extends SceneTree

# Validate imported Pond insects, their real skins and sampled movement loops.
const MANIFEST := "res://../assets/faf/pond/insects/pond_insect_001_manifest.json"

func _initialize() -> void:
	var data: Variant = JSON.parse_string(FileAccess.get_file_as_string(ProjectSettings.globalize_path(MANIFEST)))
	if not data is Dictionary or data["assets"].size() != 4:
		_fail("manifest must contain exactly four insects")
		return
	for entry in data["assets"]:
		var stem: String = entry["stem"]
		var packed := load("res://assets/faf/pond/insects/" + stem + ".glb") as PackedScene
		if packed == null:
			_fail(stem + " missing imported scene")
			return
		var model := packed.instantiate() as Node3D
		root.add_child(model)
		var skeletons: Array[Skeleton3D] = []
		var players: Array[AnimationPlayer] = []
		var meshes: Array[MeshInstance3D] = []
		_collect(model,skeletons,players,meshes)
		if skeletons.size() != 1 or players.size() != 1 or meshes.is_empty():
			_fail(stem + " missing skeleton, player, or mesh")
			return
		var skeleton := skeletons[0]
		var player := players[0]
		if skeleton.get_bone_count() != int(entry["bones"]):
			_fail(stem + " bone count")
			return
		var triangles := 0
		var materials := {}
		var bounds := AABB()
		var first := true
		for mesh in meshes:
			if mesh.skin == null or mesh.skeleton == NodePath(""):
				_fail(stem + " unskinned mesh")
				return
			bounds = mesh.get_aabb() if first else bounds.merge(mesh.get_aabb())
			first = false
			for i in mesh.mesh.get_surface_count():
				triangles += mesh.mesh.surface_get_array_index_len(i) / 3
				var mat := mesh.mesh.surface_get_material(i)
				if mat != null: materials[mat.resource_name] = true
		var dims: Array = entry["dimensions_m"]
		if triangles != int(entry["triangles"]) or materials.size() != int(entry["materials"]):
			_fail(stem + " cost differs " + str([triangles,materials.size()]))
			return
		if absf(bounds.size.x-float(dims[0]))>.0003 or absf(bounds.size.y-float(dims[2]))>.0003 or absf(bounds.size.z-float(dims[1]))>.0003:
			_fail(stem + " scale/orientation differs " + str(bounds.size))
			return
		var clip: String = entry["actions"][0]
		if not player.get_animation_list().has(clip):
			_fail(stem + " missing action " + clip)
			return
		var action := player.get_animation(clip)
		if action.length <= 0 or action.loop_mode != Animation.LOOP_LINEAR:
			_fail(stem + " action not looping")
			return
		var moving_name := "abdomen_02" if String(entry["faf_family_id"]) == "FAF-INSECT-001" else "oar_L"
		var moving := skeleton.find_bone(moving_name)
		var game_root := skeleton.find_bone("root")
		if moving < 0 or game_root < 0:
			_fail(stem + " named deform bone missing")
			return
		player.play(clip)
		player.seek(0,true);player.advance(0)
		var start_pose := skeleton.get_bone_pose(moving)
		var root_pose := skeleton.get_bone_pose(game_root)
		player.seek(action.length*.35,true);player.advance(0)
		if start_pose.is_equal_approx(skeleton.get_bone_pose(moving)):
			_fail(stem + " no sampled deformation")
			return
		if not root_pose.is_equal_approx(skeleton.get_bone_pose(game_root)):
			_fail(stem + " gameplay root moved")
			return
		player.seek(action.length,true);player.advance(0)
		if not start_pose.is_equal_approx(skeleton.get_bone_pose(moving)):
			_fail(stem + " loop seam")
			return
		print("FAF_INSECT_IMPORTED ",entry["faf_id"]," triangles=",triangles," materials=",materials.size()," bones=",skeleton.get_bone_count()," skinned_meshes=",meshes.size()," loop=",clip)
		model.queue_free()
	print("FAF_INSECT_GODOT_OK count=4")
	quit(0)

func _collect(node: Node, skeletons: Array[Skeleton3D], players: Array[AnimationPlayer], meshes: Array[MeshInstance3D]) -> void:
	if node is Skeleton3D:skeletons.append(node as Skeleton3D)
	if node is AnimationPlayer:players.append(node as AnimationPlayer)
	if node is MeshInstance3D:meshes.append(node as MeshInstance3D)
	for child in node.get_children():_collect(child,skeletons,players,meshes)

func _fail(message: String) -> void:
	printerr("FAF_INSECT_GODOT_FAILED ",message)
	quit(1)
