extends Control

@onready var main_button_container = $MainButtonContainer
@onready var start_game_menu = $StartGameMenu
@onready var animation_player = $AnimationPlayer
@onready var labyrinth_width_box = $StartGameMenu/StartGameContainer/WidthLabel/WidthSpinBox
@onready var labyrinth_length_box = $StartGameMenu/StartGameContainer/LengthLabel/LengthSpinBox


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func _on_exit_game_pressed() -> void:
	get_tree().quit()

func _on_start_game_pressed() -> void:
	animation_player.play("EnterStartGameMenu")

func _on_back_button_pressed() -> void:
	animation_player.play("ExitStartGameMenu")

func _on_start_pressed() -> void:
	var width: int = int(labyrinth_width_box.value)
	var length: int = int(labyrinth_length_box.value)
	
	Global.shared_data["width"] = width
	Global.shared_data["length"] = length
	
	get_tree().change_scene_to_file("res://main.tscn")
