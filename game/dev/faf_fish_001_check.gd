extends SceneTree

const ASSET_PATH := "res://assets/faf/fish/fish_001_yellow_tang.glb"
const EXPECTED_ACTIONS := [
	"fish_001_yellow_tang__Glide",
	"fish_001_yellow_tang__Swim",
	"fish_001_yellow_tang__SwimFast",
]


func _initialize() -> void:
	var packed: PackedScene = load(ASSET_PATH) as PackedScene
	if packed == null:
		_fail("GLB did not import as PackedScene")
		return
	var asset: Node = packed.instantiate()
	root.add_child(asset)
	var skeletons: Array[Skeleton3D] = []
	var players: Array[AnimationPlayer] = []
	var meshes: Array[MeshInstance3D] = []
	_collect(asset, skeletons, players, meshes)
	print("FAF_ROOT ", asset.name)
	for child in asset.get_children():
		print("FAF_CHILD ", child.name, " ", child.get_class())
	if skeletons.size() != 1 or skeletons[0].get_bone_count() != 14:
		_fail("expected one 14-bone Skeleton3D with paired gills")
		return
	if players.size() != 1:
		_fail("expected one AnimationPlayer")
		return
	if meshes.size() != 8:
		_fail("expected eight mesh parts")
		return
	var skeleton: Skeleton3D = skeletons[0]
	var gill_l: int = skeleton.find_bone("gill_L")
	var gill_r: int = skeleton.find_bone("gill_R")
	var head_parent: int = skeleton.find_bone("spine_01")
	if gill_l < 0 or gill_r < 0 or skeleton.get_bone_parent(gill_l) != head_parent or skeleton.get_bone_parent(gill_r) != head_parent:
		_fail("missing or incorrectly parented exported gill bones")
		return
	var player: AnimationPlayer = players[0]
	var names: PackedStringArray = player.get_animation_list()
	for expected in EXPECTED_ACTIONS:
		if not names.has(expected):
			_fail("missing Action: " + expected + "; found " + str(names))
			return
	for expected in EXPECTED_ACTIONS:
		var animation: Animation = player.get_animation(expected)
		print("FAF_ACTION ", expected, " length=", animation.length, " loop=", animation.loop_mode)
		if animation.length <= 0.0:
			_fail("empty Action: " + expected)
			return
		if animation.loop_mode != Animation.LOOP_LINEAR:
			_fail("Action does not loop: " + expected)
			return
	var head_index: int = skeleton.find_bone("spine_01")
	var tail_index: int = skeleton.find_bone("tail_fin")
	if head_index < 0 or tail_index < 0:
		_fail("head/tail bones missing")
		return
	var head_position: Vector3 = skeleton.get_bone_global_rest(head_index).origin
	var tail_position: Vector3 = skeleton.get_bone_global_rest(tail_index).origin
	print("FAF_REST head=", head_position, " tail=", tail_position, " head_minus_tail=", head_position - tail_position)
	if head_position.z <= tail_position.z:
		_fail("unexpected imported facing axis")
		return
	var bounds: AABB = meshes[0].get_aabb()
	var material_names: Dictionary = {}
	for mesh_instance in meshes:
		bounds = bounds.merge(mesh_instance.get_aabb())
		for surface in mesh_instance.mesh.get_surface_count():
			var material: Material = mesh_instance.mesh.surface_get_material(surface)
			if material != null:
				material_names[material.resource_name] = true
	print("FAF_BOUNDS ", bounds, " MATERIALS ", material_names.keys())
	var rest_tail: Transform3D = skeleton.get_bone_pose(skeleton.find_bone("tail_02"))
	player.play("fish_001_yellow_tang__Swim")
	player.seek(0.25, true)
	player.advance(0.0)
	var animated_tail: Transform3D = skeleton.get_bone_pose(skeleton.find_bone("tail_02"))
	if rest_tail.basis.is_equal_approx(animated_tail.basis):
		_fail("Swim did not deform tail_02")
		return
	if skeleton.find_bone("root") < 0 or skeleton.find_bone("spine_01") < 0:
		_fail("root/spine bone names missing")
		return
	print("FAF_MESHES ", meshes.size(), " BONES ", skeleton.get_bone_count(), " ACTIONS ", names)
	print("FAF_FISH_001_OK")
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


func _fail(message: String) -> void:
	printerr("FAF_FISH_001_FAILED ", message)
	quit(1)
