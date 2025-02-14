using System.Collections.Generic;
using Godot;

public static class WorldData
{
	public static Dictionary<Vector2I, Dictionary<Vector2I, string>> ZoneMapData = new();
	public static Vector2I PlayerLastTile = new(0, 0);

	public static bool HasZone(Vector2I worldTile)
	{
		return ZoneMapData.ContainsKey(worldTile);
	}

	public static Dictionary<Vector2I, string> GetZoneData(Vector2I worldTile)
	{
		return ZoneMapData.ContainsKey(worldTile) ? ZoneMapData[worldTile] : null;
	}

	public static void SaveZoneData(Vector2I worldTile, Dictionary<Vector2I, string> zoneData)
	{
		ZoneMapData[worldTile] = new Dictionary<Vector2I, string>(zoneData);
		GD.Print($"✅ Saved zone data for tile {worldTile}");
	}
}
