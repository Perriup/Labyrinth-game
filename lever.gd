extends Area3D

@onready var animation_player:AnimationPlayer = $Lever/AnimationPlayer
@onready var doors1 = $"../DoorFrame/Doors"
@onready var doors2 = $"../DoorFrame2/Doors"
@onready var pulled_lever = false

# Called every frame. 'delta' is the elapsed time since the previous frame.
func activate_lever() ->void:
	if (!pulled_lever):
		animation_player.play("Bones|BonesAction")
		doors1.queue_free()
		doors2.queue_free()
		pulled_lever = true
