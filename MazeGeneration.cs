using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

public partial class MazeGeneration : Node
{
	public override void _Ready()
	{
		// Generate and use the maze when the scene is ready.
		string mazeJson = GetMaze();
		GD.Print(mazeJson);
		 // You can call a function here to parse `mazeJson` and use it to build the maze in Godot.
		BuildMaze(mazeJson);
	}
	
	public string GetMaze()
	{
		Random rnd = new Random();
		int randomSeed = rnd.Next(100000);
		int width = 20;
		int height = 20;
		
		MazeCreation mazeCreation = new MazeCreation();
		Vertex[,] maze = mazeCreation.CreateMaze(randomSeed, width, height);
		
		PrintMaze(maze, width, height);
		
		return ExportMazeToJson(maze, width, height);
	}
	
	private string ExportMazeToJson(Vertex[,] maze, int width, int height)
	{
		var mazeData = new List<Dictionary<string, object>>();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
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
	
	private void BuildMaze(string mazeJson)
	{
		// Parse the maze JSON and build the maze in the Godot scene.
		var mazeData = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(mazeJson);
		
		// Iterate through maze cells
		foreach (var cell in mazeData)
		{
			int x = cell["X"].GetInt32();
			int y = cell["Y"].GetInt32();
			bool north = cell["North"].GetBoolean();
			bool south = cell["South"].GetBoolean();
			bool east = cell["East"].GetBoolean();
			bool west = cell["West"].GetBoolean();
			
			// Create a new instance of the labirinth_grid scene.
			PackedScene gridScene = (PackedScene)GD.Load("res://labirinth_grid.tscn");
			Node3D gridInstance = (Node3D)gridScene.Instantiate();
			
			// Place the grid at the correct position
			gridInstance.Position = new Vector3(x, 0, y);
			
			// Remove walls as needed
			if (north) gridInstance.GetNode("NorthWall").QueueFree();
			if (south) gridInstance.GetNode("SouthWall").QueueFree();
			if (east) gridInstance.GetNode("EastWall").QueueFree();
			if (west) gridInstance.GetNode("WestWall").QueueFree();
			
			// Add to the scene tree
			AddChild(gridInstance);
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

}
