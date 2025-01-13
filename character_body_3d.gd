extends CharacterBody3D

const SPEED = 0.8
const JUMP_VELOCITY = 8
const SENSITIVITY = 0.003

@onready var head = $Head
@onready var camera = $Head/Camera3D
@onready var head_bob: AnimationPlayer = $HeadBob
@onready var heavy_breath: AudioStreamPlayer = $HeavyBreath
@onready var raycast: RayCast3D = $Head/Camera3D/RayCast3D

func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	heavy_breath.play()

func _unhandled_input(event):
	if event is InputEventMouseMotion:
		head.rotate_y(-event.relative.x * SENSITIVITY)
		camera.rotate_x(-event.relative.y * SENSITIVITY)
		camera.rotation.x = clamp(camera.rotation.x, deg_to_rad(-60), deg_to_rad(60))

func _physics_process(delta: float) -> void:
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta

	# Handle jump.
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Get the input direction and handle the movement/deceleration.
	var input_dir = Input.get_vector("left", "right", "up", "down")
	var direction = (head.transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		velocity.x = direction.x * SPEED
		velocity.z = direction.z * SPEED
		head_bob.play("head_bob")
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)
		velocity.z = move_toward(velocity.z, 0, SPEED)
		head_bob.pause()
	
	#Handle floor change
	if Input.is_action_just_pressed("go_up_a_floor") && global_transform.origin.y < 2:
		global_transform.origin.y += 1
	if Input.is_action_just_pressed("go_down_a_floor") && global_transform.origin.y > 0.5:
		global_transform.origin.y -= 1
	move_and_slide()
	
	if Input.is_action_just_pressed("interact"):  # Bind "check_interact" in Input Map
		check_raycast_hit()
	
func check_raycast_hit():
	raycast.force_raycast_update()
	if raycast.is_colliding():
		var collider = raycast.get_collider()
		print(collider)
		if (collider.name == "RubbleStaticBody3D" && Global.shovel_collected):
			collider.queue_free()
		else:
			var i = raycast.get_collider_shape()
			var hit_node = collider.shape_owner_get_owner(i)
			if (hit_node.name == "PalletteShape"):
				collider.queue_free()
			elif(collider.has_method("activate_lever")):
				collider.activate_lever()
			
