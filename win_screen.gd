extends Control


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var width_label = $StartGameMenu/StartGameContainer/WidthLabel
	var length_label = $StartGameMenu/StartGameContainer/LengthLabel
	var time_label = $StartGameMenu/StartGameContainer/TimeLabel
	
	width_label.text = "Labyrinth width: " + str(Global.shared_data["width"])
	length_label.text = "Labyrinth length: " + str(Global.shared_data["length"])
	time_label.text = "Time taken to beat: " + Global.shared_data["time"]


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_restart_button_pressed() -> void:
	get_tree().change_scene_to_file("res://menu.tscn")
