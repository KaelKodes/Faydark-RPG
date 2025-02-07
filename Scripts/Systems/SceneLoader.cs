using Godot;
using System.Collections.Generic;

public partial class SceneLoader : Node
{
	private Dictionary<Vector2I, Node> loadedScenes = new Dictionary<Vector2I, Node>();
	private bool isOnlineMap = false; // Future MMO support
	private Node mapDisplay;
	private Vector2I playerTile;

	public override void _Ready()
	{
		mapDisplay = GetTree().Root.GetNodeOrNull("GameScene/UI/MapDisplay");
		if (mapDisplay == null)
		{
			GD.PrintErr("❌ MapDisplay not found in SceneLoader!");
			return;
		}

		GD.Print("✅ SceneLoader Ready. Monitoring Player Position...");
	}

	public void UpdateLoadedScenes(Vector2I newPlayerTile)
	{
		playerTile = newPlayerTile;
		LoadSceneAt(playerTile);
		LoadAdjacentScenes(playerTile);
	}

	private void LoadSceneAt(Vector2I tile)
	{
		if (loadedScenes.ContainsKey(tile))
		{
			GD.Print($"🔄 Scene at {tile} already loaded.");
			return;
		}

		string biomeType = World.GetBiomeAt(tile);
		PackedScene biomeScene = (PackedScene)ResourceLoader.Load($"res://Scenes/Biomes/{biomeType}.tscn");
		if (biomeScene == null)
		{
			GD.PrintErr($"❌ Failed to load biome scene: {biomeType}");
			return;
		}

		Node biomeInstance = biomeScene.Instantiate();
		mapDisplay.AddChild(biomeInstance);
		loadedScenes[tile] = biomeInstance;

		GD.Print($"✅ Loaded {biomeType} at {tile}");
	}

	private void LoadAdjacentScenes(Vector2I tile)
	{
		Vector2I[] directions = new Vector2I[]
		{
			new Vector2I(-1, 0), new Vector2I(1, 0),
			new Vector2I(0, -1), new Vector2I(0, 1)
		};

		foreach (var dir in directions)
		{
			Vector2I neighborTile = tile + dir;
			LoadSceneAt(neighborTile);
		}
	}

	public Dictionary<Vector2I, string> GetMapData()
	{
		Dictionary<Vector2I, string> mapData = new Dictionary<Vector2I, string>();
		foreach (var entry in loadedScenes)
		{
			mapData[entry.Key] = World.GetBiomeAt(entry.Key);
		}
		return mapData;
	}
}
