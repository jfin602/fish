extends SceneTree

# Isolated import proof. The four new fish are checked against their source
# manifests; FAF-FISH-001 retains its own established proof/check fixture.
const SPECS := {
	"002": ["fish_002_ocellaris_clownfish", 912, 8, 14],
	"003": ["fish_003_blue_green_chromis", 404, 8, 13],
	"004": ["fish_004_reef_butterflyfish", 896, 8, 14],
	"005": ["fish_005_blacktip_reef_shark", 876, 12, 14],
}


func _initialize() -> void:
	for id in SPECS:
		var spec: Array = SPECS[id]
		var stem: String = spec[0]
		var packed: PackedScene = load("res://assets/faf/fish/" + stem + ".glb") as PackedScene
		if packed == null:
			_fail(id, "GLB did not import")
			return
		var asset: Node3D = packed.instantiate() as Node3D
		root.add_child(asset)
		var skeletons: Array[Skeleton3D] = []
		var players: Array[AnimationPlayer] = []
		var meshes: Array[MeshInstance3D] = []
		_collect(asset, skeletons, players, meshes)
		if skeletons.size() != 1 or skeletons[0].get_bone_count() != spec[3]:
			_fail(id, "skeleton/bone count")
			return
		if players.size() != 1 or meshes.size() != spec[2]:
			_fail(id, "AnimationPlayer/mesh count")
			return
		var skeleton: Skeleton3D = skeletons[0]
		var gill_l: int = skeleton.find_bone("gill_L")
		var gill_r: int = skeleton.find_bone("gill_R")
		if id == "005":
			if gill_l >= 0 or gill_r >= 0:
				_fail(id, "shark must not import false operculum bones")
				return
		elif gill_l < 0 or gill_r < 0 or skeleton.get_bone_parent(gill_l) != skeleton.find_bone("spine_01") or skeleton.get_bone_parent(gill_r) != skeleton.find_bone("spine_01"):
			_fail(id, "missing or incorrectly parented exported gill bones")
			return
		var player: AnimationPlayer = players[0]
		var triangle_count := 0
		var materials := {}
		var bounds := AABB()
		var first_mesh := true
		for mesh_instance in meshes:
			bounds = mesh_instance.get_aabb() if first_mesh else bounds.merge(mesh_instance.get_aabb())
			first_mesh = false
			for surface in mesh_instance.mesh.get_surface_count():
				triangle_count += mesh_instance.mesh.surface_get_array_index_len(surface) / 3
				var material: Material = mesh_instance.mesh.surface_get_material(surface)
				if material != null:
					materials[material.resource_name] = true
		if triangle_count != spec[1] or materials.size() != 3:
			_fail(id, "triangle/material identity: " + str(triangle_count) + "/" + str(materials.size()))
			return
		if asset.position != Vector3.ZERO or asset.scale != Vector3.ONE:
			_fail(id, "noncanonical import transform")
			return
		var head := skeleton.find_bone("spine_01")
		var tail := skeleton.find_bone("tail_02")
		if skeleton.find_bone("root") < 0 or head < 0 or tail < 0:
			_fail(id, "root/head/tail bone names")
			return
		if skeleton.get_bone_global_rest(head).origin.z <= skeleton.get_bone_global_rest(tail).origin.z:
			_fail(id, "unexpected imported facing axis")
			return
		var action_names: PackedStringArray = player.get_animation_list()
		for role in ["Glide", "Swim", "SwimFast"]:
			var action_name: String = stem + "__" + role
			if not action_names.has(action_name):
				_fail(id, "missing Action " + action_name)
				return
			var animation: Animation = player.get_animation(action_name)
			print("FAF_ACTION ", id, " ", action_name, " length=", animation.length, " loop=", animation.loop_mode)
			if animation.length <= 0.0 or animation.loop_mode != Animation.LOOP_LINEAR:
				_fail(id, "invalid loop " + action_name)
				return
			for track in animation.get_track_count():
				if animation.track_get_type(track) == Animation.TYPE_POSITION_3D:
					_fail(id, "Action contains translation track " + action_name)
					return
			player.play(action_name)
			player.seek(0.0, true)
			player.advance(0.0)
			var initial_tail: Transform3D = skeleton.get_bone_pose(tail)
			var initial_head: Transform3D = skeleton.get_bone_pose(head)
			player.seek(animation.length * 0.25, true)
			player.advance(0.0)
			if initial_tail.basis.is_equal_approx(skeleton.get_bone_pose(tail).basis):
				_fail(id, "Action did not deform tail " + action_name)
				return
			if not initial_head.is_equal_approx(skeleton.get_bone_pose(head)):
				_fail(id, "Action moved stable head " + action_name)
				return
		print("FAF_REEF_FISH ", id, " triangles=", triangle_count, " materials=", materials.keys(),
			" meshes=", meshes.size(), " bones=", skeleton.get_bone_count(),
			" actions=", action_names, " bounds=", bounds)
		asset.queue_free()
	print("FAF_REEF_FISH_PACK_001_OK")
	quit(0)


func _collect(node: Node, skeletons: Array[Skeleton3D], players: Array[AnimationPlayer], meshes: Array[MeshInstance3D]) -> void:
	if node is Skeleton3D:
		skeletons.append(node as Skeleton3D)
	if node is AnimationPlayer:
		players.append(node as AnimationPlayer)
	if node is MeshInstance3D:
		meshes.append(node as MeshInstance3D)
	for child in node.get_children():
		_collect(child, skeletons, players, meshes)


func _fail(id: String, reason: String) -> void:
	printerr("FAF_REEF_FISH_PACK_001_FAILED ", id, ": ", reason)
	quit(1)
