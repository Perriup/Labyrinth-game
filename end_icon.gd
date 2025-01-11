extends Node3D


var start_time = 0
var end_time = 0

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var animation_player = $AnimationPlayer
	animation_player.play("spin_start")
	
	start_time = Time.get_ticks_msec()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_end_area_3d_body_entered(body: Node3D) -> void:
	if body.name == "Player":
		end_time = Time.get_ticks_msec()
		var elapsed_sec = (end_time - start_time) / 1000
		var minutes_taken = elapsed_sec / 60
		var seconds_taken = elapsed_sec % 60
		var formatted_time = str(minutes_taken) + ":" + str(seconds_taken)
		Global.shared_data["time"] = formatted_time
		
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		
		get_tree().change_scene_to_file("res://win_screen.tscn")
