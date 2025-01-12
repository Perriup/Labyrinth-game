using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

public partial class MazeGeneration : Node
{
	public override void _Ready()
	{
		var shared_data = (Godot.Collections.Dictionary)GetNode("/root/Global").Get("shared_data");
		
		int width = (int)shared_data["width"];
		int length = (int)shared_data["length"];
		
		string mazeJson = GetMaze(width, length);
		//GD.Print(mazeJson);
		BuildMaze(mazeJson, width, length);
	}
	
	public string GetMaze(int inputWidth, int inputLength)
	{
		Random rnd = new Random();
		int randomSeed = rnd.Next(100000);
		int width = inputWidth;
		int length = inputLength;
		
		MazeCreation mazeCreation = new MazeCreation();
		Vertex[,] maze = mazeCreation.CreateMaze(randomSeed, width, length);
		
		PrintMaze(maze, width, length);
		
		List<string> route = FindRoute(maze, width - 1, length - 1);
		foreach(var line in route)
		{
			GD.Print(line);
		}
		
		return ExportMazeToJson(maze, width, length);
	}
	
	private string ExportMazeToJson(Vertex[,] maze, int width, int length)
	{
		var mazeData = new List<Dictionary<string, object>>();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < length; j++)
			{
				var vertex = maze[i, j];
				mazeData.Add(new Dictionary<string, object>
				{
					{ "X", vertex.X },
					{ "Y", vertex.Y },
					{ "North", vertex.North },
					{ "South", vertex.South },
					{ "East", vertex.East },
					{ "West", vertex.West }
					});
				}
			}
		return JsonSerializer.Serialize(mazeData);
	}
	
	private void BuildMaze(string mazeJson, int width, int length)
	{
		var level_name_list = new List<string> {"res://labyrinth_grid_past.tscn", "res://labyrinth_grid_present.tscn", "res://labyrinth_grid_future.tscn"};
		
		// Parse the maze JSON and build the maze in the Godot scene.
		var maze_data = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(mazeJson);
		
		// Iterate through maze cells
		foreach (var cell in maze_data)
		{
			int x = cell["X"].GetInt32();
			// z is taken from y, because the maze is generated on a 2d xy plane, rather than 3d
			int z = cell["Y"].GetInt32();
			bool north = cell["North"].GetBoolean();
			bool south = cell["South"].GetBoolean();
			bool east = cell["East"].GetBoolean();
			bool west = cell["West"].GetBoolean();
			
			for (int y = 0; y < level_name_list.Count; y++)
			{
				// Create a new instance of the labirinth_grid scene.
				PackedScene grid_scene = (PackedScene)GD.Load(level_name_list[y]);
				Node3D grid_instance = (Node3D)grid_scene.Instantiate();
				
				// Place the grid at the correct position
				grid_instance.Position = new Vector3(x, y, z);
				
				// Remove walls as needed
				if (north) grid_instance.GetNode("NorthWall").QueueFree();
				if (south) grid_instance.GetNode("SouthWall").QueueFree();
				if (east) grid_instance.GetNode("EastWall").QueueFree();
				if (west) grid_instance.GetNode("WestWall").QueueFree();
				
				// Add to the scene tree
				AddChild(grid_instance);
			}
		}
		
		AddStart(level_name_list);
		AddEnd(width - 1, length - 1, level_name_list);
		
		PackedScene dump = (PackedScene)GD.Load("res://rubble.tscn");
		Node3D d = (Node3D)dump.Instantiate();
		
		d.Position = new Vector3(1, 0, 0);
		AddChild(d);
		
		PackedScene shovel = (PackedScene)GD.Load("res://shovel.tscn");
		Node3D s = (Node3D)shovel.Instantiate();
		
		s.Position = new Vector3(1, 1, 0);
		AddChild(s);
		
		PackedScene wall = (PackedScene)GD.Load("res://builtWall.tscn");
		Node3D w = (Node3D)wall.Instantiate();
		
		w.Position = new Vector3(2, 0, 0);
		AddChild(w);
		
		PackedScene door = (PackedScene)GD.Load("res://doors.tscn");
		Node3D dor = (Node3D)door.Instantiate();
		
		dor.Position = new Vector3(3, 0, 0);
		AddChild(dor);
	}
	
	private void AddStart(List<string> level_name_list)
	{
		for (int y = 0; y < level_name_list.Count; y++)
		{
			PackedScene start_icon_scene = (PackedScene)GD.Load("res://start_icon.tscn");
			Node3D start_instance = (Node3D)start_icon_scene.Instantiate();
			start_instance.Position = new Vector3(0, y, 0);
			AddChild(start_instance);
		}
	}
	
	private void AddEnd(int x, int z, List<string> level_name_list)
	{
		for (int y = 0; y < level_name_list.Count; y++)
		{
			PackedScene start_icon_scene = (PackedScene)GD.Load("res://end_icon.tscn");
			Node3D start_instance = (Node3D)start_icon_scene.Instantiate();
			start_instance.Position = new Vector3(x, y, z);
			AddChild(start_instance);
		}
	}
	
	internal class MazeCreation
	{
		public Vertex[,] CreateMaze(int randomSeed, int width, int height)
		{
			System.Random random = new System.Random(randomSeed);
			Vertex[,] maze = new Vertex[width, height];
			HashSet<Vertex> notVisited = new HashSet<Vertex>();
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					Vertex newVertex = new Vertex(i, j, width, height);
					maze[i, j] = newVertex;
					notVisited.Add(newVertex);
				}
			}
			Vertex firstVertex = notVisited.ElementAt(random.Next(notVisited.Count));
			notVisited.Remove(firstVertex);
			while (notVisited.Count > 0)
			{
				Vertex currentVertex = notVisited.ElementAt(random.Next(notVisited.Count));
				Queue<Vertex> currentPath = new Queue<Vertex>();
				currentPath.Enqueue(currentVertex);
				while (notVisited.Contains(currentVertex))
				{
					Vertex.AdjacentReturn adjacentReturn = currentVertex.giveAdjacent(random.Next());
					currentVertex.Direction = adjacentReturn.DirectionOfMovement;
					currentVertex = maze[adjacentReturn.X, adjacentReturn.Y];
					int firstInstance = -1;
					for (int i = 0; i < currentPath.Count; i++)
					{
						if (currentPath.ElementAt(i).Equals(currentVertex))
						{
							firstInstance = i;
							break;
						}
					}
					if (firstInstance != -1)
					{
						currentPath = new Queue<Vertex>(currentPath.Take(firstInstance + 1).ToArray());
					}
					else
					{
						currentPath.Enqueue(currentVertex);
					}
				}
				Vertex firstVertexOnPath = currentPath.Dequeue();
				notVisited.Remove(firstVertexOnPath);
				firstVertexOnPath.setBoolOnDirectionToTrue(firstVertexOnPath.Direction);
				Vertex previousVertex = firstVertexOnPath;
				while (currentPath.Count > 1)
				{
					Vertex localVertex = currentPath.Dequeue();
					notVisited.Remove(localVertex);
					localVertex.setBoolOnOpositeDirectionToTrue(previousVertex.Direction);
					localVertex.setBoolOnDirectionToTrue(localVertex.Direction);
					previousVertex = localVertex;
				}
				Vertex lastVertexOnPath = currentPath.Dequeue();
				lastVertexOnPath.Direction = previousVertex.Direction;
				notVisited.Remove(lastVertexOnPath);
				lastVertexOnPath.setBoolOnOpositeDirectionToTrue(previousVertex.Direction);
			}
			return maze;
		}
	}
	
	internal class Vertex
	{
		public Vertex(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
		public int X { get; set; }
		public int Y { get; set; }

		public int Width { get; set; }
		public int Height { get; set; }

		public enum Directions
		{
			north,
			south,
			east,
			west

		};
		public Directions Direction { get; set; }
		public bool North { get; set; }
		public bool South { get; set; }
		public bool East { get; set; }
		public bool West { get; set; }

		public void setBoolOnDirectionToTrue(Directions direction)
		{
			switch (direction)
			{
				case Directions.north:
					North = true;
					break;
				case Directions.south:
					South = true;
					break;
				case Directions.east:
					East = true;
					break;
				case Directions.west:
					West = true;
					break;
			}
		}
		public void setBoolOnOpositeDirectionToTrue(Directions direction)
		{
			switch (direction)
			{
				case Directions.north:
					South = true;
					break;
				case Directions.south:
					North = true;
					break;
				case Directions.east:
					West = true;
					break;
				case Directions.west:
					East = true;
					break;
			}
		}
		public class AdjacentReturn
		{
			public int X { get; set; }
			public int Y { get; set; }
			public Directions DirectionOfMovement { get; set; }
			public AdjacentReturn(int x, int y, Directions directionOfMovement)
			{
				X = x;
				Y = y;
				DirectionOfMovement = directionOfMovement;
			}
		}
		public AdjacentReturn giveAdjacent(int randomSeed)
		{
			System.Random random = new System.Random(randomSeed);
			Directions[] allDirections = (Directions[])Enum.GetValues(typeof(Directions));
			Directions randomDirection = (Directions)allDirections.GetValue(random.Next(4));
			bool viableAdjacent = false;
			while (!viableAdjacent)
			{
				randomDirection = (Directions)allDirections.GetValue(random.Next(4));
				viableAdjacent = true;
				if (X == 0 && randomDirection == Directions.west)
				{
					viableAdjacent = false;
				}
				else if (X == Width - 1 && randomDirection == Directions.east)
				{
					viableAdjacent = false;
				}
				else if (Y == Height - 1 && randomDirection == Directions.north)
				{
					viableAdjacent = false;
				}
				else if (Y == 0 && randomDirection == Directions.south)
				{
					viableAdjacent = false;
				}
			};
			AdjacentReturn adjacentReturn = new AdjacentReturn(X, Y, randomDirection);
			if (randomDirection == Directions.north)
			{
				adjacentReturn.Y++;
			}
			else if (randomDirection == Directions.south)
			{
				adjacentReturn.Y--;
			}
			else if (randomDirection == Directions.east)
			{
				adjacentReturn.X++;
			}
			else if (randomDirection == Directions.west)
			{
				adjacentReturn.X--;
			}
			return adjacentReturn;
		}
		public override string ToString()
		{
			String returnString = "[" + X + "," + Y + "] ";
			if (North)
				returnString += "N:T ";
			else
				returnString += "N:F ";
			if (South)
				returnString += "S:T ";
			else
				returnString += "S:F ";
			if (East)
				returnString += "E:T ";
			else
				returnString += "E:F ";
			if (West)
				returnString += "W:T ";
			else
				returnString += "W:F ";
			return returnString;
		}
	}
	
	private void PrintMaze(Vertex[,] maze, int width, int height)
	{
		string horizontalLine = ""; // Represents +-- or +  
		string verticalLine = ""; // Represents | or   

		for (int i = height - 1; i >= 0; i--) // Top to bottom
		{
			verticalLine += "|"; // Start a new vertical line

			for (int j = 0; j < width; j++) // Left to right
			{
				// Check if we are in the topmost row
				if (i == height - 1)
				{
					// Always print +-- for the topmost horizontal line
					horizontalLine += "+--";
				}
				else
				{
					// Print +-- if there's no northern connection, otherwise +  
					if (maze[j, i].North)
					{
						horizontalLine += "+  ";
					}
					else
					{
						horizontalLine += "+--";
					}
				}

				// End of column, add a vertical bar
				if (j == width - 1)
				{
					horizontalLine += "+";
					verticalLine += "  |"; // End of row
				}
				else
				{
					// Print | if there's no eastern connection, otherwise a space
					if (maze[j, i].East)
					{
						verticalLine += "   ";
					}
					else
					{
						verticalLine += "  |";
					}
				}
			}

			// Print the horizontal line and the vertical line for the current row
			GD.Print(horizontalLine);
			horizontalLine = ""; // Reset for the next row

			GD.Print(verticalLine);
			verticalLine = ""; // Reset for the next row
		}

		// Print the bottom-most horizontal line
		string bottomLine = "";
		for (int i = 0; i < width; i++)
		{
			bottomLine += "+--";
		}
		bottomLine += "+";
		GD.Print(bottomLine);
	}
	
	private List<string> FindRoute(Vertex[,] maze, int endX, int endY)
	{
		Stack<Vertex> stack = new Stack<Vertex>();
		HashSet<Vertex> visited = new HashSet<Vertex>();
		Dictionary<Vertex, Vertex> cameFrom = new Dictionary<Vertex, Vertex>(); // Used in tracing the correct path
		List<string> route = new List<string>();

		Vertex start = maze[0, 0];
		Vertex end = maze[endX, endY];

		stack.Push(start);

		while (stack.Count > 0)
		{
			Vertex current = stack.Pop();
			
			// If this vertex is already visited, skip it
			if (visited.Contains(current))
				continue;
			
			visited.Add(current);
			
			// If we reached the end, backtrack to find the route
			if (current == end)
			{
				Vertex backtrack = current;
				while (backtrack != null)
				{
					route.Insert(0, $"[{backtrack.X}, {backtrack.Y}]");
					cameFrom.TryGetValue(backtrack, out backtrack);
				}
				break;
			}
			
			// Add unvisited neighbors and track where they came from
			if (current.North && current.Y + 1 < maze.GetLength(1) && !visited.Contains(maze[current.X, current.Y + 1]))
			{
				stack.Push(maze[current.X, current.Y + 1]);
				if (!cameFrom.ContainsKey(maze[current.X, current.Y + 1]))
					cameFrom[maze[current.X, current.Y + 1]] = current;
			}
			
			if (current.South && current.Y - 1 >= 0 && !visited.Contains(maze[current.X, current.Y - 1]))
			{
				stack.Push(maze[current.X, current.Y - 1]);
				if (!cameFrom.ContainsKey(maze[current.X, current.Y - 1]))
					cameFrom[maze[current.X, current.Y - 1]] = current;
			}
			
			if (current.East && current.X + 1 < maze.GetLength(0) && !visited.Contains(maze[current.X + 1, current.Y]))
			{
				stack.Push(maze[current.X + 1, current.Y]);
				if (!cameFrom.ContainsKey(maze[current.X + 1, current.Y]))
					cameFrom[maze[current.X + 1, current.Y]] = current;
			}
			
			if (current.West && current.X - 1 >= 0 && !visited.Contains(maze[current.X - 1, current.Y]))
			{
				stack.Push(maze[current.X - 1, current.Y]);
				if (!cameFrom.ContainsKey(maze[current.X - 1, current.Y]))
					cameFrom[maze[current.X - 1, current.Y]] = current;
			}
		}
		
		// If the route list is empty, no path was found
		if (route.Count == 0)
		{
			route.Add("No route found.");
		}
		
		return route;
	}
}
