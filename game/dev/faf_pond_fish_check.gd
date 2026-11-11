extends SceneTree

# Focused isolated import/playback proof for Pond FAF-FISH-006–010.
const SPECIES := {
	"006": "fish_006_bluegill",
	"007": "fish_007_largemouth_bass",
	"008": "fish_008_channel_catfish",
	"009": "fish_009_common_carp",
	"010": "fish_010_northern_pike",
}
const STAGES := ["egg", "larva", "juvenile", "subadult", "adult"]
const SWIM_ROLES := ["Idle", "Glide", "SwimSlow", "Swim", "SwimFast", "TurnLeft", "TurnRight"]
const EGG_ROLES := ["Idle", "Hatch"]


func _initialize() -> void:
	var manifest_path: String = ProjectSettings.globalize_path("res://").path_join("../assets/faf/pond/fish/pond_fish_001_manifest.json")
	var manifest_text := FileAccess.get_file_as_string(manifest_path)
	var manifest: Variant = JSON.parse_string(manifest_text)
	if not manifest is Dictionary or not manifest.has("assets"):
		_fail("Pond pack", "missing or malformed source manifest")
		return
	var expected := {}
	for record in manifest["assets"]:
		expected[record["stem"]] = record
	var count := 0
	for id in SPECIES:
		for stage in STAGES:
			var stem: String = SPECIES[id] + "__" + stage
			var packed: PackedScene = load("res://assets/faf/pond/fish/" + stem + ".glb") as PackedScene
			if packed == null:
				_fail(stem, "GLB import failed")
				return
			var asset: Node3D = packed.instantiate() as Node3D
			root.add_child(asset)
			if asset.position != Vector3.ZERO or asset.rotation != Vector3.ZERO or asset.scale != Vector3.ONE:
				_fail(stem, "nonidentity imported root transform")
				return
			var skeletons: Array[Skeleton3D] = []
			var players: Array[AnimationPlayer] = []
			var meshes: Array[MeshInstance3D] = []
			_collect(asset, skeletons, players, meshes)
			if skeletons.size() != 1 or players.size() != 1 or meshes.is_empty():
				_fail(stem, "missing deform skeleton, player or geometry")
				return
			var skeleton: Skeleton3D = skeletons[0]
			var player: AnimationPlayer = players[0]
			var required: Array = ["root", "shell_l", "shell_r", "embryo"] if stage == "egg" else [
				"root", "spine_01", "spine_02", "spine_03", "tail_01", "tail_02",
				"tail_fin", "pectoral_l", "pectoral_r", "dorsal", "anal", "gill_L", "gill_R"]
			for name in required:
				if skeleton.find_bone(name) < 0:
					_fail(stem, "missing deform bone " + name)
					return
			var triangles := 0
			var materials := {}
			var bounds := AABB()
			var first := true
			for mesh_instance in meshes:
				bounds = mesh_instance.get_aabb() if first else bounds.merge(mesh_instance.get_aabb())
				first = false
				for surface in mesh_instance.mesh.get_surface_count():
					triangles += mesh_instance.mesh.surface_get_array_index_len(surface) / 3
					var material: Material = mesh_instance.mesh.surface_get_material(surface)
					if material != null:
						materials[material.resource_name] = true
			if triangles <= 0 or materials.size() < 2 or bounds.size.x <= 0 or bounds.size.y <= 0 or bounds.size.z <= 0:
				_fail(stem, "invalid geometry/material bounds")
				return
			if not expected.has(stem):
				_fail(stem, "missing manifest record")
				return
			var record: Dictionary = expected[stem]
			if triangles != int(record["triangles"]) or materials.size() != int(record["materials"]) or skeleton.get_bone_count() != int(record["bones"]):
				_fail(stem, "imported triangles/materials/bones differ from manifest")
				return
			var dimensions: Array = record["dimensions_m"]
			# Imported Pond geometry is nose-first toward +Z; player presentation corrects to -Z.
			if absf(bounds.size.x - float(dimensions[0])) > 0.0001 or absf(bounds.size.y - float(dimensions[2])) > 0.0001 or absf(bounds.size.z - float(dimensions[1])) > 0.0001:
				_fail(stem, "imported dimensions differ from manifest")
				return
			var roles: Array = EGG_ROLES if stage == "egg" else SWIM_ROLES
			if stage != "egg":
				var head_z: float = skeleton.get_bone_global_rest(skeleton.find_bone("spine_01")).origin.z
				var tail_z: float = skeleton.get_bone_global_rest(skeleton.find_bone("tail_fin")).origin.z
				if head_z <= tail_z:
					_fail(stem, "imported nose is not +Z ahead of tail")
					return
			for role_value in roles:
				var role: String = str(role_value)
				var action_name: String = stem + "__" + role
				if not player.get_animation_list().has(action_name):
					_fail(stem, "missing Action " + role)
					return
				var action: Animation = player.get_animation(action_name)
				var should_loop: bool = role in ["Idle", "Glide", "SwimSlow", "Swim", "SwimFast"]
				if action.length <= 0 or action.loop_mode != (Animation.LOOP_LINEAR if should_loop else Animation.LOOP_NONE):
					_fail(stem, "wrong duration/loop on " + role)
					return
				player.play(action_name)
				player.seek(0.0, true)
				player.advance(0.0)
				var root_pose: Transform3D = skeleton.get_bone_pose(skeleton.find_bone("root"))
				var moving_bone: String = "embryo" if stage == "egg" and role == "Idle" else "shell_l" if stage == "egg" else "tail_02"
				var moving_start: Transform3D = skeleton.get_bone_pose(skeleton.find_bone(moving_bone))
				var head_start := Transform3D.IDENTITY
				if stage != "egg":
					head_start = skeleton.get_bone_pose(skeleton.find_bone("spine_01"))
				player.seek(action.length * (1.0 if role == "Hatch" else .5 if role.begins_with("Turn") else .25), true)
				player.advance(0.0)
				if not root_pose.is_equal_approx(skeleton.get_bone_pose(skeleton.find_bone("root"))):
					_fail(stem, "game root moved in " + role)
					return
				if moving_start.is_equal_approx(skeleton.get_bone_pose(skeleton.find_bone(moving_bone))):
					_fail(stem, "no visible deform control motion in " + role)
					return
				if stage != "egg" and not head_start.is_equal_approx(skeleton.get_bone_pose(skeleton.find_bone("spine_01"))):
					_fail(stem, "head moved in " + role)
					return
			print("FAF_POND_IMPORTED ", stem, " triangles=", triangles, " materials=", materials.size(),
			      " bones=", skeleton.get_bone_count(), " actions=", roles.size())
			asset.queue_free()
			count += 1
	if count != 25 or expected.size() != 25:
		_fail("Pond pack", "expected 25 imports")
		return
	print("FAF_POND_GODOT_OK count=", count)
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


func _fail(asset: String, reason: String) -> void:
	printerr("FAF_POND_GODOT_FAILED ", asset, ": ", reason)
	quit(1)
