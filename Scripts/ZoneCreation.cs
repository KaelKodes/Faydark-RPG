using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class ZoneCreation : Node2D
{
	private Dictionary<int, bool> passableEdges = new Dictionary<int, bool>(); // 6 edges
	private const int HEX_RADIUS = 32;
	private const int ZONE_EDGE_LENGTH = 10;
	private Vector2 dragStart;
	private bool dragging = false;

	private Random rng = new Random();
	private FastNoiseLite noiseGenerator;
	private Dictionary<Vector2I, float> noiseMap;

	private PackedScene hexScene = (PackedScene)ResourceLoader.Load("res://Scenes/World/UI/HexTile.tscn");

	private Dictionary<Vector2I, string> zoneMap = new Dictionary<Vector2I, string>();
	private string biomeType = "plains";
	public static ZoneCreation Instance { get; private set; }

	private static HashSet<string> passableBiomes = new HashSet<string>
	{
		"grass", "forest", "swamp", "desert", "hills"
	};
	private Dictionary<string, Color> terrainColors = new Dictionary<string, Color>
{
	{ "grass", new Color(0.4f, 0.8f, 0.2f) },    // 🌿 Light Green
	{ "tree", new Color(0.0f, 0.5f, 0.0f) },     // 🌲 Dark Green
	{ "rock", new Color(0.5f, 0.5f, 0.5f) },     // 🪨 Grey
	{ "water", new Color(0.1f, 0.3f, 0.8f) },    // 🌊 Blue
	{ "sand", new Color(0.9f, 0.8f, 0.4f) },     // 🏜️ Yellow
	{ "path", new Color(0.6f, 0.4f, 0.2f) }      // 🛤️ Brownish Path Color
};


	private static bool IsBiomePassable(string biome) => passableBiomes.Contains(biome);

	public override async void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
			GD.Print("✅ ZoneCreation Singleton Initialized.");
		}
		else
		{
			QueueFree();
			return;
		}

		biomeType = World.GetBiomeAt(World.playerTile);
		if (string.IsNullOrEmpty(biomeType) || biomeType == "Unknown")
		{
			GD.PrintErr($"❌ Invalid biome found at {World.playerTile}. Defaulting to Wilderness.");
			biomeType = "wilderness";
		}

		GD.Print($"🌍 Generating Zone for Biome: {biomeType}");

		GenerateNoiseMap();
		await GenerateZone();
	}

	private void GenerateNoiseMap()
	{
		noiseGenerator = new FastNoiseLite();
		noiseGenerator.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
		noiseGenerator.SetFrequency(0.05f);

		noiseMap = new Dictionary<Vector2I, float>();

		for (int q = -ZONE_EDGE_LENGTH; q <= ZONE_EDGE_LENGTH; q++)
		{
			for (int r = -ZONE_EDGE_LENGTH; r <= ZONE_EDGE_LENGTH; r++)
			{
				if (Mathf.Abs(q + r) > ZONE_EDGE_LENGTH) continue;

				float noiseValue = noiseGenerator.GetNoise2D(q, r);
				noiseMap[new Vector2I(q, r)] = Mathf.Clamp((noiseValue + 1) / 2, 0, 1);
			}
		}
	}

	private async Task GenerateZone()
	{
		zoneMap.Clear();
		CheckPassableEdges();

		for (int q = -ZONE_EDGE_LENGTH; q <= ZONE_EDGE_LENGTH; q++)
		{
			for (int r = -ZONE_EDGE_LENGTH; r <= ZONE_EDGE_LENGTH; r++)
			{
				int s = -q - r;
				if (Mathf.Max(Mathf.Max(Mathf.Abs(q), Mathf.Abs(r)), Mathf.Abs(s)) > ZONE_EDGE_LENGTH)
   				 continue;


				Vector2I hexPos = new Vector2I(q, r);
				zoneMap[hexPos] = GetRandomTerrainForBiome(biomeType, q, r);
			}
		}

		GeneratePaths();
		await RenderZone();
	}

	private void CheckPassableEdges()
	{
		Vector2I playerTile = World.playerTile;
		for (int direction = 0; direction < 6; direction++)
		{
			Vector2I neighborPos = GetNeighborPosition(direction, playerTile);
			string neighborBiome = World.GetBiomeAt(neighborPos);
			passableEdges[direction] = IsBiomePassable(neighborBiome);
		}
	}

	private void GeneratePaths()
	{
		Vector2I start = new Vector2I(0, 0);
		foreach (int direction in passableEdges.Keys)
		{
			if (!passableEdges[direction]) continue;

			Vector2I target = GetEdgePosition(direction);
			CreateWindingPath(start, target);
		}
	}

	private void CreateWindingPath(Vector2I start, Vector2I end)
	{
		Vector2I current = start;
		while (current != end)
		{
			int dx = end.X > current.X ? 1 : (end.X < current.X ? -1 : 0);
			int dy = end.Y > current.Y ? 1 : (end.Y < current.Y ? -1 : 0);

			if (rng.Next(100) < 30) dx = 0;
			if (rng.Next(100) < 30) dy = 0;

			current = new Vector2I(current.X + dx, current.Y + dy);
			zoneMap[current] = "path";
		}
	}

	private async Task RenderZone()
	{
		foreach (var pos in zoneMap.Keys)
		{
			Vector2 pixelPos = AxialToPixel(pos.X, pos.Y);

			Node2D hexInstance = (Node2D)hexScene.Instantiate();
			hexInstance.Position = pixelPos;

			string terrainType = zoneMap[pos];
			hexInstance.Modulate = terrainColors.ContainsKey(terrainType) ? terrainColors[terrainType] : new Color(1, 1, 1);

			AddChild(hexInstance);
			await Task.Delay(1);
		}

		GD.Print("✅ Zone rendering complete!");
	}

	private string GetRandomTerrainForBiome(string biome, int q, int r)
	{
		if (!noiseMap.ContainsKey(new Vector2I(q, r))) return "grass";

		float noiseValue = noiseMap[new Vector2I(q, r)];

		switch (biome)
		{
			case "forest":
				return noiseValue < 0.4f ? "tree" : (noiseValue < 0.7f ? "grass" : "rock");

			case "mountain":
				return noiseValue < 0.3f ? "rock" : (noiseValue < 0.6f ? "grass" : "tree");

			case "lake":
				return noiseValue < 0.8f ? "water" : "grass";

			case "swamp":
				return noiseValue < 0.5f ? "water" : (noiseValue < 0.8f ? "grass" : "tree");

			case "wilderness":
			default:
				return noiseValue < 0.4f ? "grass" : (noiseValue < 0.6f ? "tree" : "rock");
		}
	}

	private static readonly Vector2I[] hexDirections = new Vector2I[]
	{
		new Vector2I(1, 0), new Vector2I(1, -1), new Vector2I(0, -1),
		new Vector2I(-1, 0), new Vector2I(-1, 1), new Vector2I(0, 1)
	};

	private Vector2I GetNeighborPosition(int direction, Vector2I currentTile)
	{
		if (direction < 0 || direction >= hexDirections.Length)
			return currentTile;

		return new Vector2I(currentTile.X + hexDirections[direction].X, currentTile.Y + hexDirections[direction].Y);
	}

	private Vector2I GetEdgePosition(int direction)
	{
		if (direction < 0 || direction >= hexDirections.Length)
			return new Vector2I(0, 0);

		return new Vector2I(hexDirections[direction].X * ZONE_EDGE_LENGTH, hexDirections[direction].Y * ZONE_EDGE_LENGTH);
	}

	private Vector2 AxialToPixel(int q, int r)
	{
		float x = HEX_RADIUS * Mathf.Sqrt(3) * (q + 0.5f * r);
		float y = HEX_RADIUS * 1.5f * r;
		return new Vector2(x, y);
	}
}
