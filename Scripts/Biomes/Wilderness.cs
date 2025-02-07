using Godot;
using System.Collections.Generic;

public partial class Wilderness : Node2D
{
	[Export] public TileMapLayer TileMapLayer;
	[Export] public Node2D Props;
	[Export] public NavigationRegion2D Navigation;
	[Export] public Node2D EdgeMarkers;

	private Dictionary<string, float> spawnChances = new Dictionary<string, float>
	{
		{ "Tree", 0.3f },
		{ "Rock", 0.1f },
		{ "Rock2", 0.07f },
		{ "Rock3", 0.05f },
		{ "Bush", 0.1f },
		{ "Bush2", 0.07f },
		{ "Bush3", 0.05f },
		{ "Log", 0.05f },
		{ "Mushroom", 0.03f }
	};

	public override void _Ready()
{
	GD.Print("🌲 Wilderness Scene Loaded");

	if (TileMapLayer == null)
	{
		GD.PrintErr("❌ TileMapLayer is NULL! Check if it's assigned in the scene.");
		return;
	}

	GenerateEnvironment();
}


	private void GenerateEnvironment()
{
	GD.Print("🌿 Generating Environment...");

	if (TileMapLayer == null)
	{
		GD.PrintErr("❌ TileMapLayer is NULL! Cannot generate environment.");
		return;
	}

	foreach (Vector2I cell in TileMapLayer.GetUsedCellsById(0))
	{
		GD.Print($"📌 Processing cell: {cell}");
		PlaceRandomProp(cell);
	}
}





	private void PlaceRandomProp(Vector2I cell)
	{
		foreach (var prop in spawnChances)
		{
			if (GD.Randf() < prop.Value)
			{
				string path = $"res://Sprites/Object/{GetPropFolder(prop.Key)}/{prop.Key}.tscn";
				PackedScene scene = (PackedScene)ResourceLoader.Load(path);
				if (scene != null)
				{
					Node2D instance = (Node2D)scene.Instantiate();
					instance.Position = TileMapLayer.MapToLocal(cell);
					Props.AddChild(instance);
					GD.Print($"🌿 Spawned {prop.Key} at {cell}");
				}
				else
				{
					GD.PrintErr($"❌ Failed to load: {path}");
				}
			}
		}
	}

	private string GetPropFolder(string propName)
	{
		if (propName.Contains("Rock")) return "Rocks";
		if (propName.Contains("Bush")) return "Bushes";
		return "Clutter"; // Default folder for Log, Mushroom, etc.
	}
}
