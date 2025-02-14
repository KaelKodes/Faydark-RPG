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


	private Button nextButton;

	private Vector2 dragStart;
	private bool dragging = false;

	public static string[,] biomeMap; // ✅ Now biomeMap is static
	private PackedScene hexScene = (PackedScene)ResourceLoader.Load("res://Scenes/World/UI/HexTile.tscn");
	private Dictionary<Vector2I, Node2D> hexInstances = new Dictionary<Vector2I, Node2D>();
	public static Dictionary<Vector2I, string> worldMap = new Dictionary<Vector2I, string>(); // Stores biome data
	public static Vector2I playerTile = new Vector2I(25, 25); // Player starts at (0,1)
	[Export] private Camera2D camera; // ✅ Assign this in the Godot editor
	private Vector2 wildernessPosition = new Vector2(25, 25); // ✅ Adjust to actual location


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
	public static bool biomeMapReady = false; // ✅ Add this flag

	public static World Instance { get; private set; }
	public bool IsReady { get; private set; } = false;

	public override async void _Ready()
	{
		GD.Print("🌍 Starting World Generation...");

		await GenerateHexGrid();
		await Task.Delay(1000); // ✅ Shorter delay to ensure hexes exist
		await GenerateForestsAndSwamps();
		await Task.Delay(1000);
		await FinalizeDeepWaterAndMountains();

		biomeMapReady = true;
		IsReady = true;
		GD.Print("✅ World Generation Complete!");

		await Task.Delay(500); // ✅ Small delay to prevent async issues

		GD.Print("🎥 Panning camera to Wilderness...");
		PanCameraTo(new Vector2(1450, 1200), 3.0f); // ✅ Replace with the actual Wilderness tile position

		GD.Print("🎮 Waiting for NextButton...");
		nextButton = GetNode<Button>("NextButton");
		nextButton.Pressed += OnNextButtonPressed;
	}


	private void OnNextButtonPressed()
	{
		GD.Print("🎮 Player clicked Next! Transitioning to GameScene...");
		// ✅ Enable GameScene & SceneManager before switching scenes
		var gameScene = GetTree().Root.GetNodeOrNull("GameScene");
		var sceneManager = GetTree().Root.GetNodeOrNull("SceneManager");

		if (gameScene != null)
		{
			GD.Print("✅ Enabling GameScene...");
			((GameScene)gameScene).EnableGameScene(); // ✅ Activate GameScene
		}

		if (sceneManager != null)
		{
			GD.Print("✅ Enabling SceneManager...");
			((SceneManager)sceneManager).EnableSceneManager(); // ✅ Activate SceneManager
		}
		GetTree().ChangeSceneToFile("res://Scenes/UI/GameScene.tscn");
	}

	private async Task PanCameraTo(Vector2 targetPosition, float duration)
	{
		if (camera == null)
		{
			GD.PrintErr("❌ Camera2D not found!");
			return;
		}

		Vector2 startPos = camera.GlobalPosition;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			await ToSignal(GetTree(), "process_frame"); // Wait for next frame

			float delta = (float)GetProcessDeltaTime(); // Correct way to get delta time
			elapsed += delta;
			float t = Mathf.Clamp(elapsed / duration, 0f, 1f); // Ensure t stays between 0 and 1

			camera.GlobalPosition = startPos.Lerp(targetPosition, t);
		}

		camera.GlobalPosition = targetPosition; // Ensure exact final position
		GD.Print("✅ Camera pan complete!");

		// 🔍 Call the zoom-in function after the pan is done
		await ZoomCamera(1.5f, 3.0f); // Zooms in to 1.5x over 5 seconds
	}

	private async Task ZoomCamera(float targetZoom, float duration)
	{
		if (camera == null)
		{
			GD.PrintErr("❌ Camera2D not found!");
			return;
		}

		GD.Print("🔍 Zooming in...");
		Vector2 startZoom = camera.Zoom;
		Vector2 endZoom = new Vector2(targetZoom, targetZoom);
		float elapsed = 0f;

		while (elapsed < duration)
		{
			await ToSignal(GetTree(), "process_frame");

			float delta = (float)GetProcessDeltaTime();
			elapsed += delta;
			float t = Mathf.Clamp(elapsed / duration, 0f, 1f);

			camera.Zoom = startZoom.Lerp(endZoom, t);
		}

		camera.Zoom = endZoom; // Ensure exact zoom level
		GD.Print($"✅ Zoom-in complete! New Zoom: {camera.Zoom}");
	}

	public static Vector2 GetTilePosition(Vector2I tile)
	{
		return new Vector2(
			HEX_RADIUS * Mathf.Sqrt(3) * (tile.X + 0.5f * (tile.Y % 2)),
			HEX_RADIUS * 1.5f * tile.Y
		);
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

		// Set 6: Set Global as Instance as True
		if (Instance == null)
		{
			Instance = this;
			GD.Print("✅ World Singleton Initialized.");
		}
		else
		{
			QueueFree(); // Ensures only one instance exists
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
	public static string GetBiomeAt(Vector2I tilePosition)
	{
		if (biomeMap == null)
		{
			GD.PrintErr("❌ biomeMap is NULL!");
			return "Unknown";
		}

		// ✅ FIX: Ensure only valid tiles are checked
		if (!IsValidTile(tilePosition))
		{
			GD.PrintErr($"❌ Tile {tilePosition} is out of bounds! Defaulting to wilderness.");
			return "wilderness";
		}

		if (string.IsNullOrEmpty(biomeMap[tilePosition.X, tilePosition.Y]))
		{
			GD.PrintErr($"❌ Biome not assigned for Tile {tilePosition}! Defaulting to wilderness.");
			return "wilderness";
		}

		return biomeMap[tilePosition.X, tilePosition.Y];
	}

	// ✅ Utility function to check tile validity
	private static bool IsValidTile(Vector2I tile)
	{
		return tile.X >= 0 && tile.X < GRID_WIDTH && tile.Y >= 0 && tile.Y < GRID_HEIGHT;
	}

	private static bool IsPositionWithinBounds(Vector2I tilePosition)
	{
		return tilePosition.X >= 0 && tilePosition.X < GRID_WIDTH &&
			   tilePosition.Y >= 0 && tilePosition.Y < GRID_HEIGHT;
	}






	public static void SetPlayerPosition(Vector2I newPosition)
	{
		playerTile = newPosition;
		GD.Print($"🚶 Player moved to Tile {newPosition}, Biome: {GetBiomeAt(newPosition)}");
	}

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
	#region Data
	// ✅ Dictionary to store generated zones
	private static Dictionary<Vector2I, ZoneCreation> savedZones = new Dictionary<Vector2I, ZoneCreation>();

	// ✅ Check if a zone exists
	public static bool ZoneExists(Vector2I tilePosition)
	{
		return savedZones.ContainsKey(tilePosition);
	}

	// ✅ Retrieve an existing zone
	public static ZoneCreation GetZone(Vector2I tilePosition)
	{
		if (savedZones.TryGetValue(tilePosition, out ZoneCreation zone))
		{
			if (zone == null)
			{
				GD.PrintErr($"❌ ERROR: Zone at {tilePosition} was stored as NULL!");
				return null;
			}
			GD.Print($"✅ Loaded existing zone for tile {tilePosition}");
			return zone;
		}
		GD.PrintErr($"❌ ERROR: GetZone() - No zone found at {tilePosition}!");
		return null;
	}


	// ✅ Save a new zone
	public static void SaveZone(Vector2I tilePosition, ZoneCreation zone)
	{
		if (!savedZones.ContainsKey(tilePosition))
		{
			savedZones[tilePosition] = zone;
			GD.Print($"✅ Saved zone data for tile {tilePosition}");
		}
	}
	// ✅ Remove an existing zone from memory
public static void RemoveZone(Vector2I tilePosition)
{
	if (savedZones.ContainsKey(tilePosition))
	{
		GD.Print($"🗑️ Removing zone at {tilePosition}");
		savedZones.Remove(tilePosition);
	}
}

// ✅ Convert a tile's position to a zone coordinate
public static Vector2I GetZoneForTile(Vector2I tilePosition)
{
	int zoneSize = 10; // 👈 Adjust based on actual zone size
	int zoneX = tilePosition.X / zoneSize;
	int zoneY = tilePosition.Y / zoneSize;
	return new Vector2I(zoneX, zoneY);
}


	#endregion
}
