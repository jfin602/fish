extends Node3D

# Collision is deliberately independent from the display GLB. Hollow pieces
# use separate radial wall slabs and have no end cap or convex hull.
@export var faf_id := ""
@export var family := ""
@export var length_m := 1.0
@export var radius_m := 1.0
@export var inner_radius_m := 0.0
@export var height_m := 1.0
@export var bend_m := 0.0
@export var fork_count := 0


func _ready() -> void:
	set_meta("faf_id", faf_id)
	set_meta("faf_habitat", "pond")
	if family == "cluster":
		return
	var body := StaticBody3D.new()
	body.name = "SimpleWoodCollision"
	body.collision_layer = 1
	body.collision_mask = 0
	add_child(body)
	if family == "hollow_trunk":
		var sides := 16 if inner_radius_m > 1.5 else 12
		var a := PI / float(sides)
		var collision_inner := inner_radius_m * 0.93
		var middle := (radius_m + collision_inner) * 0.5 * cos(a)
		for i in range(sides):
			var angle := TAU * (float(i) + 0.5) / float(sides)
			var slab := BoxShape3D.new()
			slab.size = Vector3((radius_m - collision_inner) * cos(a) + 0.04,
				2.0 * radius_m * sin(a) + 0.025, length_m * 0.975)
			var node := CollisionShape3D.new()
			node.name = "Wall_%02d" % i
			node.shape = slab
			node.position = Vector3(middle * cos(angle), radius_m + middle * sin(angle), 0)
			node.rotation.z = angle
			body.add_child(node)
	elif family == "fallen_log":
		var shape := CylinderShape3D.new()
		shape.radius = radius_m * 0.88
		shape.height = length_m * 0.92
		var node := CollisionShape3D.new()
		node.name = "Trunk"
		node.shape = shape
		node.position.y = radius_m
		node.rotation.x = PI * 0.5
		body.add_child(node)
		# The two substantial broken forks on B/C are solid cover. Thin
		# rootlets remain decoration so they cannot snag the player collider.
		for i in range(fork_count):
			var sign_value := 1.0 if i % 2 == 0 else -1.0
			var t := .28 + .43 * float(i) / maxf(1.0,float(fork_count))
			var source_y := length_m * (t - .5)
			var source_x := bend_m * sin(PI * t)
			var start := Vector3(source_x,radius_m * 1.65,-source_y)
			var tip := Vector3(source_x + sign_value * (1.4 + float(i)*.25),
				radius_m * 1.65 + .22 + float(i)*.2,-(source_y + .9))
			var fork := CollisionShape3D.new()
			fork.name = "MajorFork_%d" % i
			var capsule := CapsuleShape3D.new()
			capsule.radius = radius_m * .24
			capsule.height = maxf(start.distance_to(tip),capsule.radius*2)
			fork.shape = capsule
			fork.position = (start + tip)*.5
			fork.quaternion = Quaternion(Vector3.UP,(tip-start).normalized())
			body.add_child(fork)
	elif family == "stump":
		var shape := CylinderShape3D.new()
		shape.radius = radius_m * 0.8
		shape.height = height_m * 0.92
		var node := CollisionShape3D.new()
		node.name = "Stem"
		node.shape = shape
		node.position.y = height_m * 0.5
		body.add_child(node)
