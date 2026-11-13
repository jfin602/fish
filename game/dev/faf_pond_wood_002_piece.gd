extends Node3D

# Imported mesh and collision share the Blender source recipe, but remain
# separate nodes. Open shell panels never gain a closing convex hull.
@export var faf_id := ""
@export var collision_json := ""


func _ready() -> void:
	set_meta("faf_id",faf_id)
	set_meta("faf_habitat","pond")
	var shapes: Array = JSON.parse_string(collision_json)
	assert(shapes != null and not shapes.is_empty())
	var body := StaticBody3D.new()
	body.name = "AuthoredWoodCollision"
	body.collision_layer = 1
	body.collision_mask = 0
	add_child(body)
	for i in shapes.size():
		var info: Dictionary = shapes[i]
		var node := CollisionShape3D.new()
		node.name = "WoodShape_%03d" % i
		if info["kind"] == "capsule":
			var start := _from_blender(info["a"])
			var finish := _from_blender(info["b"])
			var span := finish - start
			var radius := float(info["radius"])
			var capsule := CapsuleShape3D.new()
			capsule.radius = radius
			capsule.height = maxf(span.length(),radius*2.0)
			node.shape = capsule
			node.position = (start+finish)*.5
			node.quaternion = Quaternion(Vector3.UP,span.normalized())
		else:
			assert(info["kind"] == "shell_slab")
			var angle := float(info["angle"])
			var outer := float(info["outer"])
			var inner := float(info["inner"])
			var half_angle := PI/float(info["sides"])
			var middle := (outer+inner)*.5*cos(half_angle)
			var slab := BoxShape3D.new()
			slab.size = Vector3((outer-inner)*cos(half_angle)+.025,
				2.0*outer*sin(half_angle)+.015,float(info["length"]))
			node.shape = slab
			node.position = Vector3(middle*cos(angle),outer+middle*sin(angle),
				-float(info["y"]))
			node.rotation.z = angle
		body.add_child(node)


func _from_blender(point: Array) -> Vector3:
	return Vector3(float(point[0]),float(point[2]),-float(point[1]))
