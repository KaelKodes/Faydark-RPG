using Godot;
using System.Collections.Generic;

public partial class GameState : Node
{
	private static GameState _instance;
	public static GameState Instance => _instance ??= new GameState();

	private Dictionary<Vector2I, string> zoneBiomes = new Dictionary<Vector2I, string>(); // Stores biomes per zone
	private static Dictionary<Vector2I, Dictionary<Vector2I, string>> SavedZones = new Dictionary<Vector2I, Dictionary<Vector2I, string>>();
	private Dictionary<Vector2I, ZoneCreation> loadedZones = new Dictionary<Vector2I, ZoneCreation>(); // Stores generated zones
	private static CharacterData SavedPlayerData;

	public static Vector2I LastZone { get; set; } // Stores last visited zone

	public override void _Ready()
	{
		if (_instance == null)
		{
			_instance = this;
			GD.Print("✅ GameState Initialized.");
		}
		else
		{
			GD.PrintErr("❌ Duplicate GameState detected!");
			QueueFree();
		}
	}

	// ✅ Returns the player's current zone (NOT tile-based)
	public static Vector2I GetCurrentZone()
	{
		return ConvertTileToZone(World.playerTile);
	}

	// ✅ Converts a tile position to a zone ID
	public static Vector2I ConvertTileToZone(Vector2I tile)
	{
		int zoneX = Mathf.FloorToInt(tile.X / (float)ZoneCreation.ZONE_EDGE_LENGTH);
		int zoneY = Mathf.FloorToInt(tile.Y / (float)ZoneCreation.ZONE_EDGE_LENGTH);
		return new Vector2I(zoneX, zoneY);
	}

	// ✅ Checks if a zone has already been generated
	public static bool ZoneExists(Vector2I zone)
	{
		return Instance.loadedZones.ContainsKey(zone);
	}

	// ✅ Saves a zone so it persists
	public static void SaveZone(Vector2I zone, ZoneCreation zoneData) // ✅ Expect ZoneCreation instead
{
	if (!Instance.loadedZones.ContainsKey(zone))
	{
		Instance.loadedZones[zone] = zoneData; // ✅ Now storing `ZoneCreation`
		GD.Print($"✅ Saved zone object for {zone}");
	}
}






	// ✅ Retrieves a zone if it was already generated
	public static ZoneCreation GetZone(Vector2I zone)
	{
		return Instance.loadedZones.ContainsKey(zone) ? Instance.loadedZones[zone] : null;
	}

	// ✅ Removes a zone if it's being unloaded
	public static void RemoveZone(Vector2I zone)
	{
		if (Instance.loadedZones.ContainsKey(zone))
		{
			Instance.loadedZones.Remove(zone);
			GD.Print($"🗑️ Removed zone data for {zone}");
		}
	}

	// ✅ Stores biome data for each zone
	public static void SaveBiomeData(Vector2I zone, string biome)
	{
		if (!Instance.zoneBiomes.ContainsKey(zone))
		{
			Instance.zoneBiomes[zone] = biome;
			GD.Print($"✅ Biome for {zone} saved as {biome}");
		}
	}

	// ✅ Gets the biome for a zone
	public static string GetBiomeForZone(Vector2I zone)
	{
		return Instance.zoneBiomes.ContainsKey(zone) ? Instance.zoneBiomes[zone] : "wilderness";
	}

	public static string GetZoneData(Vector2I zone)
{
	if (!Instance.loadedZones.ContainsKey(zone))
	{
		GD.PrintErr($"❌ ERROR: Zone data not found for {zone}!");
		return "wilderness"; // Default biome if zone data is missing
	}

	return Instance.loadedZones[zone].BiomeType; // ✅ Ensure each zone only has ONE biome
}


public static bool PlayerExists()
{
	return CharacterData.Instance != null;
}


public static Character LoadPlayer()
{
	if (GameState.SavedPlayerData == null) // ✅ No instance reference
	{
		GD.PrintErr("❌ No saved player data found!");
		return null;
	}

	GD.Print("✅ Loading player from saved data...");
	Character player = new Character();
	player.LoadData(GameState.SavedPlayerData); // ✅ Direct access to static variable
	return player;
}



}
