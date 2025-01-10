extends Node3D

# Path to the 1x1_floor_with_walls scene
@export var floor_with_walls_scene: PackedScene
# Reference to the player node
@export var player: Node3D

func _ready():
	# Spawn the floor with walls
	var first_cell = spawn_floor_with_walls(Vector3(0, 0, 0))
	
	# Spawn the second cell next to it
	var second_cell = spawn_floor_with_walls(Vector3(1, 0, 0))  # Adjust position as needed

	# Remove the walls between the two cells
	remove_walls_between_cells(first_cell, second_cell)

func spawn_floor_with_walls(position: Vector3) -> Node3D:
	# Instantiate the scene
	var instance = floor_with_walls_scene.instantiate()
	add_child(instance)
	return instance
	
func remove_walls_between_cells(cell_a: Node3D, cell_b: Node3D) -> void:
	# Identify and remove walls between cells
	var wall_a = cell_a.get_node("EastWall")  # Replace with your node path for the east wall
	var wall_b = cell_b.get_node("WestWall")  # Replace with your node path for the west wall

	if wall_a:
		wall_a.queue_free()  # Remove the wall from the first cell
	if wall_b:
		wall_b.queue_free()  # Remove the wall from the second cell
