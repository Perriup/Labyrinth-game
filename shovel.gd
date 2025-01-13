extends Node3D

func _on_area_3d_body_entered(body: Node3D) -> void:
	if body.name == "Player":
		Global.shovels_collected = Global.shovels_collected + 1
		
		visible = false
		$Area3D/CollisionShape3D.disabled = true
		
		var label = body.get_node("Head/Camera3D/CanvasLayer/Label")
		label.text = "Shovels: " + str(Global.shovels_collected)
		label.visible = true
		
		await get_tree().create_timer(2.0).timeout
		label.visible = false
		
		queue_free()
