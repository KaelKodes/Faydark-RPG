using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class ZoneCreation : Node2D
{
	private const int HEX_RADIUS = 32;
	public const int ZONE_EDGE_LENGTH = 10;

	private Random rng = new Random();
	private FastNoiseLite noiseGenerator;
	private Dictionary<Vector2I, float> noiseMap;

	private PackedScene hexScene = (PackedScene)ResourceLoader.Load("res://Scenes/World/UI/HexTile.tscn");

	public Dictionary<Vector2I, string> zoneMap = new Dictionary<Vector2I, string>();
	private string biomeType = "wilderness"; // Default biome
	public string BiomeType { get; private set; }


	public static ZoneCreation Instance { get; private set; }

	private static HashSet<string> passableBiomes = new HashSet<string>
	{
		"grass", "forest", "swamp", "desert", "hills"
	};

	private Dictionary<string, Color> terrainColors = new Dictionary<string, Color>
	{
		{ "grass", new Color(0.4f, 0.8f, 0.2f) },
		{ "tree", new Color(0.0f, 0.5f, 0.0f) },
		{ "rock", new Color(0.5f, 0.5f, 0.5f) },
		{ "water", new Color(0.1f, 0.3f, 0.8f) },
		{ "sand", new Color(0.9f, 0.8f, 0.4f) },
		{ "path", new Color(0.6f, 0.4f, 0.2f) }
	};

	[Signal]
	public delegate void ZoneRenderedEventHandler();

	public override async void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
			GD.Print("✅ ZoneCreation Initialized.");
		}
		else
		{
			QueueFree();
			return;
		}

		Vector2I currentZone = GameState.GetCurrentZone();

		if (GameState.ZoneExists(currentZone))
{
	GD.Print($"🗺️ Loading existing zone for {currentZone}");
	biomeType = GameState.GetZoneData(currentZone); // ✅ Get a string, not a dictionary
}
else
{
	GD.Print($"🌍 Generating new zone for {currentZone}");
	biomeType = GameState.GetBiomeForZone(currentZone);

	GenerateNoiseMap();
	await GenerateZone();

			// ✅ Create a dictionary for storing tile biomes in this zone
Dictionary<Vector2I, string> zoneData = new Dictionary<Vector2I, string>();

// ✅ Populate the dictionary with biome data for each tile
foreach (var tilePos in zoneMap.Keys) // ✅ Iterate over existing tiles in the zoneMap
{
	zoneData[tilePos] = BiomeType; // Store the biome type per tile
}

// ✅ Now correctly pass the dictionary to SaveZone
GameState.SaveZone(currentZone, this);

		}

		await RenderZone();
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

		for (int q = -ZONE_EDGE_LENGTH; q <= ZONE_EDGE_LENGTH; q++)
		{
			for (int r = -ZONE_EDGE_LENGTH; r <= ZONE_EDGE_LENGTH; r++)
			{
				int s = -q - r;
				if (Mathf.Max(Mathf.Max(Mathf.Abs(q), Mathf.Abs(r)), Mathf.Abs(s)) > ZONE_EDGE_LENGTH) continue;

				Vector2I hexPos = new Vector2I(q, r);
				zoneMap[hexPos] = GetRandomTerrainForBiome(biomeType, q, r);
			}
		}
	}
	public void GenerateZoneForBiome(string biome)
{
	GD.Print($"🌍 ZoneCreation: Generating Zone for Biome: {biome}");

	biomeType = biome; // ✅ Set the biome type
	GenerateNoiseMap();
	_ = GenerateZone(); // ✅ Call the async zone generation method
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
		EmitSignal(nameof(ZoneRendered));
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

	private Vector2 AxialToPixel(int q, int r)
	{
		float x = HEX_RADIUS * Mathf.Sqrt(3) * (q + 0.5f * r);
		float y = HEX_RADIUS * 1.5f * r;
		return new Vector2(x, y);
	}
}
