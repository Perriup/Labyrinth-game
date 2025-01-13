extends Area3D

const FOLLOW_SPEED = 0.5
@onready var animation_player:AnimationPlayer = $Ghast/AnimationPlayer

func _process(delta: float) -> void:
	if (Global.character_movement.size() == 0 || !Global.enable_controls):
		return
		
	if (global_transform.origin.y != Global.level):
		global_transform.origin.y = Global.level
		
	# Get the current target position
	var target_pos_2d = Global.character_movement[0]  # 2D vector (x, z)
	var target_pos_3d = Vector3(target_pos_2d.x, Global.level, target_pos_2d.y)  # Convert to 3D
	
	# Calculate the direction to the target
	var direction = (target_pos_3d - global_transform.origin).normalized()
	
	# Rotate to face the target
	look_at(target_pos_3d, Vector3.UP)
	
	# Move towards the target
	if global_transform.origin.distance_to(target_pos_3d) > 0.1:
		var move_step = direction * FOLLOW_SPEED * delta
		global_transform.origin += move_step
	else:
		Global.character_movement.pop_front()

func _on_body_entered(body: Node3D) -> void:
	if body.name == "Player":
		var player_camera = body.get_node("Head/Camera3D")
		var look_at_target = global_transform.origin + Vector3(0, 0.6, 0)
		player_camera.look_at(look_at_target, Vector3.UP)
		Global.enable_controls = false
		animation_player.play("shooting")
		animation_player.seek(1.3)
		$AudioStreamPlayer3D2.play()
		
		await get_tree().create_timer(4.0).timeout
		call_deferred("change_scene_to_menu")

func change_scene_to_menu():
	get_tree().change_scene_to_file("res://menu.tscn")
