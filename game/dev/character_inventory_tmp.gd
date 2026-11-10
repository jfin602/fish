extends SceneTree

func _initialize() -> void:
	for path in [
		"res://assets/faf/fish/fish_001_yellow_tang.glb",
		"res://assets/faf/fish/fish_002_ocellaris_clownfish.glb",
		"res://assets/faf/fish/fish_003_blue_green_chromis.glb",
		"res://assets/faf/fish/fish_004_reef_butterflyfish.glb",
		"res://assets/faf/fish/fish_005_blacktip_reef_shark.glb",
		"res://assets/vendor-local/quaternius/Fish1.fbx",
		"res://assets/vendor-local/quaternius/Fish2.fbx",
		"res://assets/vendor-local/quaternius/Fish3.fbx",
		"res://assets/vendor-local/quaternius/Manta ray.fbx",
		"res://assets/vendor-local/quaternius/Dolphin.fbx",
		"res://assets/vendor-local/quaternius/Shark.fbx",
		"res://assets/vendor-local/quaternius/Whale.fbx"]:
		var scene: PackedScene = load(path)
		if scene == null:
			print("MISSING ", path)
			continue
		var model: Node3D = scene.instantiate()
		root.add_child(model)
		var found: Array[String] = []
		var box := AABB()
		var first := true
		for node in model.find_children("*", "MeshInstance3D", true, false):
			var mesh := node as MeshInstance3D
			if mesh.mesh == null: continue
			var mb := mesh.get_aabb()
			box = mb if first else box.merge(mb)
			first = false
			found.append("%s@%s" % [mesh.name, mb.get_center()])
		var clips: Array[String] = []
		for node in model.find_children("*", "AnimationPlayer", true, false):
			var player := node as AnimationPlayer
			clips.append_array(player.get_animation_list())
		var bones: Array[String] = []
		for node in model.find_children("*", "Skeleton3D", true, false):
			var sk := node as Skeleton3D
			for i in sk.get_bone_count(): bones.append(sk.get_bone_name(i))
		print("MODEL ", path, " meshes=", found, " aabb=", box, " clips=", clips, " bones=", bones)
		model.free()
	quit()
