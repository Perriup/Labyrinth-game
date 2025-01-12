extends Area3D

@onready var animation_player:AnimationPlayer = $Lever/AnimationPlayer
@onready var doors = $"../Doors"

# Called every frame. 'delta' is the elapsed time since the previous frame.
func activate_lever() ->void:
	animation_player.play("Bones|BonesAction")
	doors.queue_free()
