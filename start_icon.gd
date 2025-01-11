extends Node3D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var animation_player = $AnimationPlayer
	animation_player.play("spin_start")


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass