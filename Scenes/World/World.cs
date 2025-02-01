using System.Linq;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public partial class World : Node2D
{
	private const int HEX_RADIUS = 32; // Size of each hex tile
	private const int GRID_WIDTH = 50; // Number of hexes horizontally
	private const int GRID_HEIGHT = 50; // Number of hexes vertically

	private Vector2 dragStart;
	private bool dragging = false;

	private string[,] biomeMap;
	private PackedScene hexScene = (PackedScene)ResourceLoader.Load("res://Scenes/World/UI/HexTile.tscn");
	private Dictionary<Vector2I, Node2D> hexInstances = new Dictionary<Vector2I, Node2D>();


	#region Biome Data
	private class BiomeData
	{
		public Color Color;
		public List<string> AllowedNeighbors;
		public BiomeData(Color color, List<string> allowedNeighbors)
		{
			Color = color;
			AllowedNeighbors = allowedNeighbors;
		}
	}

	private Dictionary<string, BiomeData> biomeData = new Dictionary<string, BiomeData>
	{
		{ "mountain", new BiomeData(new Color(0.5f, 0.5f, 0.5f), new List<string> { "forest", "wilderness", "swamp" }) },
		{ "lake", new BiomeData(new Color(0.12f, 0.56f, 1f), new List<string> { "forest", "swamp" }) },
		{ "wilderness", new BiomeData(new Color(0.5f, 0f, 0.5f), new List<string> { "forest", "mountain", "lake", "swamp" }) },
		{ "plains", new BiomeData(new Color(0.0f, 0.5f, 0.0f), new List<string> { "forest", "mountain", "lake", "swamp", "wilderness" }) },
		{ "forest", new BiomeData(new Color(0.3f, 0.4f, 0.2f), new List<string> { "plains", "lake", "mountain", "swamp" }) },
		{ "swamp", new BiomeData(new Color(0.5f, 0.3f, 0.1f), new List<string> { "forest", "lake", "mountain" }) },
		{ "deep_water", new BiomeData(new Color(0.02f, 0.1f, 0.4f), new List<string> { "lake" }) },
		{ "tall_mountain", new BiomeData(new Color(0.3f, 0.3f, 0.3f), new List<string> { "mountain" }) }
	};
	#endregion
	#region Input Handling
	public override void _Input(InputEvent @event)
	{
		Camera2D camera = GetNode<Camera2D>("Camera2D");

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp) // Zoom in
			{
				camera.Zoom *= 0.9f; // Zoom in (smaller values = closer)
				camera.Zoom = camera.Zoom.Clamp(new Vector2(0.5f, 0.5f), new Vector2(2f, 2f)); // Limit zoom
			}
			else if (mouseEvent.ButtonIndex == MouseButton.WheelDown) // Zoom out
			{
				camera.Zoom *= 1.1f; // Zoom out (larger values = farther)
				camera.Zoom = camera.Zoom.Clamp(new Vector2(0.5f, 0.5f), new Vector2(2f, 2f)); // Limit zoom
			}
		}
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
			{
				dragging = true;
				dragStart = GetGlobalMousePosition();
			}
			else if (!mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
			{
				dragging = false;
			}
		}

		if (@event is InputEventMouseMotion mouseMotion && dragging)
		{
			camera.Position -= mouseMotion.Relative;
		}
	}
	#endregion

	#region World Generation
	public override async void _Ready()
	{
		await GenerateHexGrid();
		await Task.Delay(1500);
		await GenerateForestsAndSwamps();
		await Task.Delay(1500);
		await FinalizeDeepWaterAndMountains();
	}

	private async Task GenerateHexGrid()
{
	biomeMap = new string[GRID_WIDTH, GRID_HEIGHT];
	hexInstances.Clear(); // ✅ Ensure no duplicate instances
	List<Node2D> newHexes = new List<Node2D>(); // ✅ Store instances before adding them

	// Step 1: Generate Wilderness
	int startX = GRID_WIDTH / 2;
	int startY = GRID_HEIGHT / 2;
	GrowBiomeCluster(startX, startY, "wilderness", GD.RandRange(1, 3));

	// Step 2: Generate Lakes and Mountains
	GenerateLargeBiomeClusters("lake", 40, 90, 2, 3);
	GenerateLargeBiomeClusters("mountain", 50, 120, 3, 5);

	// Step 2.5: Ensure all tiles are assigned a biome before rendering
	for (int x = 0; x < GRID_WIDTH; x++)
	{
		for (int y = 0; y < GRID_HEIGHT; y++)
		{
			if (string.IsNullOrEmpty(biomeMap[x, y]))
			{
				//GD.PrintErr($"WARNING: Unassigned tile at {x},{y}. Assigning default biome.");
				biomeMap[x, y] = "plains"; // Default biome assignment
			}
		}
	}

	// Step 3: Generate hex instances (MULTI-THREADED, but WITHOUT scene modifications)
	Parallel.For(0, GRID_WIDTH, x =>
	{
		for (int y = 0; y < GRID_HEIGHT; y++)
		{
			Vector2I tilePos = new Vector2I(x, y);
			Node2D hexInstance = (Node2D)hexScene.Instantiate();

			lock (hexInstances) // ✅ Prevents race conditions
			{
				hexInstances[tilePos] = hexInstance;
			}

			hexInstance.Name = $"HexTile_{x}_{y}";
			hexInstance.Position = AxialToPixel(x, y);
			hexInstance.Modulate = new Color(1, 1, 1, 0); // Start transparent

			lock (newHexes) // ✅ Store for safe scene modification later
			{
				newHexes.Add(hexInstance);
			}
		}
	});

	// Step 4: SAFELY Add Nodes to Scene (MUST be on main thread)
	foreach (var hex in newHexes)
{
	AddChild(hex); // ✅ Now it's safe!

	// Get the correct grid position for this hex
	Vector2I gridPos = hexInstances.FirstOrDefault(kv => kv.Value == hex).Key;
	ApplyBiome(hex, biomeMap[gridPos.X, gridPos.Y]); // ✅ Now we use the correct grid coordinates!
}


	// Step 5: Fade in tiles after placing them
	foreach (var pos in hexInstances.Keys)
	{
		await FadeIn(hexInstances[pos]);
	}
}



	// Generates larger biomes, ensuring they aren't placed too close to each other
	private void GenerateLargeBiomeClusters(string biome, int minSize, int maxSize, int minCount, int maxCount)
	{
		Random random = new Random();
		List<Vector2I> placedClusters = new List<Vector2I>();
		int clusterCount = random.Next(minCount, maxCount + 1);

		for (int i = 0; i < clusterCount; i++)
		{
			Vector2I start;
			int attempts = 0;

			do
			{
				start = new Vector2I(random.Next(GRID_WIDTH), random.Next(GRID_HEIGHT));
				attempts++;
			}
			while ((biomeMap[start.X, start.Y] != null || IsTooCloseToOtherClusters(start, placedClusters, 4)) && attempts < 20);

			GrowBiomeCluster(start.X, start.Y, biome, random.Next(minSize, maxSize + 1));
			placedClusters.Add(start);
		}
	}

	// Prevents clusters from being placed too close to each other
	private bool IsTooCloseToOtherClusters(Vector2I pos, List<Vector2I> existingClusters, int minDistance)
	{
		foreach (Vector2I cluster in existingClusters)
		{
			if (pos.DistanceTo(cluster) < minDistance)
				return true;
		}
		return false;
	}

	// Grows a biome cluster from a starting point
	private void GrowBiomeCluster(int startX, int startY, string biome, int size)
	{
		Queue<Vector2I> toExpand = new Queue<Vector2I>();
		toExpand.Enqueue(new Vector2I(startX, startY));

		int count = 0;
		while (toExpand.Count > 0 && count < size)
		{
			Vector2I pos = toExpand.Dequeue();
			if (biomeMap[pos.X, pos.Y] != null) continue; // Already assigned

			biomeMap[pos.X, pos.Y] = biome;
			count++;

			foreach (Vector2I neighbor in GetHexNeighbors(pos.X, pos.Y))
			{
				if (biomeMap[neighbor.X, neighbor.Y] == null && new Random().NextDouble() < 0.7) // 70% chance to expand
					toExpand.Enqueue(neighbor);
			}
		}
	}

	private string ChooseBiome(Vector2I tile)
	{
		List<string> possibleBiomes = new List<string>();
		foreach (Vector2I neighbor in GetHexNeighbors(tile.X, tile.Y))
		{
			if (biomeMap[neighbor.X, neighbor.Y] != null)
			{
				possibleBiomes.AddRange(biomeData[biomeMap[neighbor.X, neighbor.Y]].AllowedNeighbors);
			}
		}
		return possibleBiomes.Count > 0 ? possibleBiomes[(int)(GD.Randi() % possibleBiomes.Count)] : "plains";
	}

	private List<Vector2I> GetHexNeighbors(int x, int y)
	{
		List<Vector2I> neighbors = new List<Vector2I>();
		int[] dx = { 1, 1, 0, -1, -1, 0 };
		int[] dyEven = { 0, 1, 1, 1, 0, -1 };
		int[] dyOdd = { -1, 0, 1, 0, -1, -1 };
		int[] dy = (x % 2 == 0) ? dyEven : dyOdd;

		for (int i = 0; i < 6; i++)
		{
			int nx = x + dx[i];
			int ny = y + dy[i];
			if (nx >= 0 && nx < GRID_WIDTH && ny >= 0 && ny < GRID_HEIGHT)
				neighbors.Add(new Vector2I(nx, ny));
		}

		return neighbors;
	}

	private void ApplyBiome(Node2D hexTile, string biome)
	{
		if (string.IsNullOrEmpty(biome) || !biomeData.ContainsKey(biome))
		{
			GD.PrintErr($"ERROR: Invalid biome '{biome}' detected. Assigning fallback 'plains'.");
			biome = "plains";  // ✅ Default biome to prevent errors
		}

		Polygon2D fill = hexTile.GetNode<Polygon2D>("Fill");
		fill.Color = biomeData[biome].Color;
	}
	private Vector2 AxialToPixel(int x, int y)
	{
		float xOffset = HEX_RADIUS * Mathf.Sqrt(3) * (x + 0.5f * (y % 2));
		float yOffset = HEX_RADIUS * 1.5f * y;
		return new Vector2(xOffset, yOffset);
	}
	private async Task GenerateForestsAndSwamps()
	{
		Random random = new Random();
		foreach (var pos in hexInstances.Keys)
		{
			if (biomeMap[pos.X, pos.Y] == "plains") // ✅ Only modify plains, not existing biomes
			{
				double chance = random.NextDouble();
				if (chance < 0.60) biomeMap[pos.X, pos.Y] = "forest";
				else if (chance < 0.61) biomeMap[pos.X, pos.Y] = "swamp";
			}
		}

		foreach (var pos in hexInstances.Keys)
		{
			ApplyBiome(hexInstances[pos], biomeMap[pos.X, pos.Y]);
			await FadeIn(hexInstances[pos]);
		}
	}


	private async Task FinalizeDeepWaterAndMountains()
	{
		List<Vector2I> toUpdate = new List<Vector2I>();

		foreach (var pos in hexInstances.Keys)
		{
			// Ensure Deep Water is inside a Lake AND has no Plains within 2 tiles
			if (biomeMap[pos.X, pos.Y] == "lake" && CountNeighbors(pos, "lake") >= 3 &&
				AllNeighborsAre(pos, new[] { "lake", "deep_water" }) &&
				NoNearbyPlains(pos, 3))
			{
				toUpdate.Add(pos);
			}
		}

		foreach (var pos in toUpdate)
			biomeMap[pos.X, pos.Y] = "deep_water";

		toUpdate.Clear(); // Reset for Tall Mountains

		foreach (var pos in hexInstances.Keys)
		{
			// Ensure Tall Mountains are inside Mountains AND have no Plains within 2 tiles
			if (biomeMap[pos.X, pos.Y] == "mountain" && CountNeighbors(pos, "mountain") >= 3 &&
				AllNeighborsAre(pos, new[] { "mountain", "tall_mountain" }) &&
				NoNearbyPlains(pos, 2))
			{
				toUpdate.Add(pos);
			}
		}

		foreach (var pos in toUpdate)
			biomeMap[pos.X, pos.Y] = "tall_mountain";

		// Apply biome changes with fade-in
		foreach (var pos in hexInstances.Keys)
		{
			ApplyBiome(hexInstances[pos], biomeMap[pos.X, pos.Y]);
			await FadeIn(hexInstances[pos]);
		}
	}

	#endregion

	#region Utilities
	private async Task FadeIn(Node2D hex)
	{
		float alpha = 0;
		while (alpha < 1)
		{
			alpha += 0.1f;
			hex.Modulate = new Color(1, 1, 1, alpha);
			//await Task.Delay(1);
		}
	}

	private int CountNeighbors(Vector2I pos, string biome)
	{
		return GetHexNeighbors(pos.X, pos.Y).Count(n => biomeMap[n.X, n.Y] == biome);
	}

	// Ensures all neighbors match allowed types
	private bool AllNeighborsAre(Vector2I pos, string[] allowedTypes)
	{
		foreach (var neighbor in GetHexNeighbors(pos.X, pos.Y))
		{
			if (!allowedTypes.Contains(biomeMap[neighbor.X, neighbor.Y]))
				return false;
		}
		return true;
	}

	// Ensures there's no Plains within a strict radius
	private bool NoNearbyPlains(Vector2I pos, int radius)
	{
		Queue<Vector2I> toCheck = new Queue<Vector2I>();
		HashSet<Vector2I> checkedTiles = new HashSet<Vector2I>();

		toCheck.Enqueue(pos);
		checkedTiles.Add(pos);

		while (toCheck.Count > 0 && radius > 0)
		{
			int count = toCheck.Count;
			radius--;

			for (int i = 0; i < count; i++)
			{
				Vector2I current = toCheck.Dequeue();
				foreach (var neighbor in GetHexNeighbors(current.X, current.Y))
				{
					if (!checkedTiles.Contains(neighbor))
					{
						if (biomeMap[neighbor.X, neighbor.Y] == "plains")
						{
							//GD.Print($"❌ Plains found near {pos}, preventing deep water/tall mountain placement.");
							return false; // Found plains tile within radius
						}
						toCheck.Enqueue(neighbor);
						checkedTiles.Add(neighbor);
					}
				}
			}
		}
		return true;
	}


	#endregion
}
