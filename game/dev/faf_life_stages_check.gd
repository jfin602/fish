extends SceneTree

# Isolated import/playback check for the twenty new stage GLBs and five
# unchanged canonical adults. This script does not touch the player or world.
const SPECIES := {
	"001": "fish_001_yellow_tang",
	"002": "fish_002_ocellaris_clownfish",
	"003": "fish_003_blue_green_chromis",
	"004": "fish_004_reef_butterflyfish",
	"005": "fish_005_blacktip_reef_shark",
}
const BONY_STAGES := ["egg", "larva", "juvenile", "subadult", "adult"]
const SHARK_STAGES := ["newborn_pup", "early_juvenile", "juvenile", "subadult", "adult"]


func _initialize() -> void:
	var checked := 0
	for id in SPECIES:
		var stages: Array = SHARK_STAGES if id == "005" else BONY_STAGES
		for stage in stages:
			var stem: String = SPECIES[id] if stage == "adult" else SPECIES[id] + "__" + stage
			var packed: PackedScene = load("res://assets/faf/fish/" + stem + ".glb") as PackedScene
			if packed == null:
				_fail(stem, "GLB did not import")
				return
			var asset: Node3D = packed.instantiate() as Node3D
			root.add_child(asset)
			var skeletons: Array[Skeleton3D] = []
			var players: Array[AnimationPlayer] = []
			var meshes: Array[MeshInstance3D] = []
			_collect(asset, skeletons, players, meshes)
			if asset.position != Vector3.ZERO or asset.rotation != Vector3.ZERO or asset.scale != Vector3.ONE:
				_fail(stem, "nonidentity import transform")
				return
			if meshes.is_empty():
				_fail(stem, "no imported geometry")
				return
			var bounds := AABB()
			var first := true
			var triangles := 0
			var materials := {}
			for mesh_instance in meshes:
				bounds = mesh_instance.get_aabb() if first else bounds.merge(mesh_instance.get_aabb())
				first = false
				for surface in mesh_instance.mesh.get_surface_count():
					triangles += mesh_instance.mesh.surface_get_array_index_len(surface) / 3
					var material: Material = mesh_instance.mesh.surface_get_material(surface)
					if material != null:
						materials[material.resource_name] = true
			if triangles <= 0 or bounds.size.x <= 0.0 or bounds.size.y <= 0.0 or bounds.size.z <= 0.0:
				_fail(stem, "invalid geometry bounds/triangles")
				return
			if stage == "egg":
				if skeletons.size() != 0 or players.size() != 0 or materials.size() != 2:
					_fail(stem, "egg imported a skeleton/Action or wrong materials")
					return
			else:
				if skeletons.size() != 1 or players.size() != 1 or materials.size() < 2:
					_fail(stem, "swimming stage skeleton/player/materials")
					return
				var skeleton: Skeleton3D = skeletons[0]
				for bone in ["root", "spine_01", "tail_01", "tail_fin", "pectoral_l", "pectoral_r"]:
					if skeleton.find_bone(bone) < 0:
						_fail(stem, "missing bone " + bone)
						return
				if stage != "adult" or id != "005":
					for gill in ["gill_L", "gill_R"]:
						if skeleton.find_bone(gill) < 0:
							_fail(stem, "missing gill control " + gill)
							return
				elif skeleton.find_bone("gill_L") >= 0 or skeleton.find_bone("gill_R") >= 0:
					_fail(stem, "canonical adult shark unexpectedly changed")
					return
				var player: AnimationPlayer = players[0]
				var actions: PackedStringArray = player.get_animation_list()
				for role in ["Glide", "Swim", "SwimFast"]:
					var action_name: String = stem + "__" + role
					if not actions.has(action_name):
						_fail(stem, "missing Action " + action_name)
						return
					var action: Animation = player.get_animation(action_name)
					print("FAF_LIFE_ACTION ", action_name, " length=", action.length, " loop=", action.loop_mode)
					if action.length <= 0.0 or action.loop_mode != Animation.LOOP_LINEAR:
						_fail(stem, "invalid loop " + action_name)
						return
					for track in action.get_track_count():
						if action.track_get_type(track) == Animation.TYPE_POSITION_3D:
							_fail(stem, "translation track " + action_name)
							return
					player.play(action_name)
					player.seek(0.0, true)
					player.advance(0.0)
					var tail: int = skeleton.find_bone("tail_01")
					var head: int = skeleton.find_bone("spine_01")
					var tail_start: Transform3D = skeleton.get_bone_pose(tail)
					var head_start: Transform3D = skeleton.get_bone_pose(head)
					player.seek(action.length * .25, true)
					player.advance(0.0)
					if tail_start.basis.is_equal_approx(skeleton.get_bone_pose(tail).basis):
						_fail(stem, "tail did not animate in " + action_name)
						return
					if not head_start.is_equal_approx(skeleton.get_bone_pose(head)):
						_fail(stem, "head moved in " + action_name)
						return
			print("FAF_LIFE_STAGE ", stem, " triangles=", triangles,
			      " materials=", materials.size(), " bones=", skeletons[0].get_bone_count() if skeletons.size() else 0)
			asset.queue_free()
			checked += 1
	if checked != 25:
		_fail("library", "expected 25 representations")
		return
	print("FAF_LIFE_STAGES_GODOT_OK count=", checked)
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


func _fail(stem: String, reason: String) -> void:
	printerr("FAF_LIFE_STAGES_GODOT_FAILED ", stem, ": ", reason)
	quit(1)
