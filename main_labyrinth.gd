extends Node3D

# Mesh library resource
@export var mesh_library: MeshLibrary

# Mesh IDs from the library
@export var floor_mesh_id: int = 0
@export var wall_mesh_id: int = 1

# Grid dimensions and cell size
@export var grid_size: int = 20
@export var cell_size: Vector3 = Vector3(1, 1, 1)

# Reference to the player node
@export var player: Node3D

func _ready() -> void:
	initial_map()
	place_player()

func initial_map():
	# Generate the map
	for x in range(grid_size):
		for z in range(grid_size):
			# Place a floor tile at every position
			place_mesh(floor_mesh_id, Vector3(x * cell_size.x, 0, z * cell_size.z))

			# Place walls at the boundaries
			if x == 0 or x == grid_size - 1 or z == 0 or z == grid_size - 1:
				# Determine wall position based on its orientation
				var position = Vector3(x * cell_size.x, cell_size.y / 2, z * cell_size.z)
				
				if x == 0:  # Left boundary
					position.x -= cell_size.x / 2  # Move wall to the left edge of the cell
				elif x == grid_size - 1:  # Right boundary
					position.x += cell_size.x / 2  # Move wall to the right edge of the cell
				elif z == 0:  # Top boundary
					position.z -= cell_size.z / 2  # Move wall to the top edge of the cell
				elif z == grid_size - 1:  # Bottom boundary
					position.z += cell_size.z / 2  # Move wall to the bottom edge of the cell
					
				# Place the wall with adjusted position and rotation
				place_mesh(wall_mesh_id, position, rotation)

func place_mesh(mesh_id: int, position: Vector3, rotation: Vector3 = Vector3(0, 0, 0)) -> void:
	# Get mesh from library
	var mesh = mesh_library.get_item_mesh(mesh_id)
	if mesh != null:
		# Create a StaticBody3D to hold the collision
		var body = StaticBody3D.new()
		add_child(body)
		
		# Add the mesh instance to the StaticBody3D
		var instance = MeshInstance3D.new()
		instance.mesh = mesh
		body.add_child(instance)
		
		# Add a collision shape (BoxShape3D as an example)
		var collision_shape = CollisionShape3D.new()
		collision_shape.shape = BoxShape3D.new()
		collision_shape.shape.extents = cell_size / 2  # Adjust size as needed
		body.add_child(collision_shape)
		
		# Set the position and rotation of the wall
		body.global_transform.origin = position
		body.global_transform.basis = Basis().rotated(Vector3(0, 1, 0), rotation.y)
	else:
		print("Mesh ID", mesh_id, "not found in the MeshLibrary!")

func place_player() -> void:
	if player:
		# Set player position above the first cell (0, 0) of the labyrinth
		var spawn_position = Vector3(0, 10, 0)
		player.global_transform.origin = spawn_position
	else:
		print("Player node is not assigned!")
