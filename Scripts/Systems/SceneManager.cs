using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SceneManager : Node
{

	private static SceneManager _instance;
	public static SceneManager Instance
	{
		get
		{
			if (_instance == null)
				GD.PrintErr("❌ SceneManager.Instance accessed before it was initialized!");
			return _instance;
		}
	}

	private Dictionary<string, string> biomeScenes = new Dictionary<string, string>
	{
		{ "wilderness", "res://Scenes/Biomes/Wilderness.tscn" },
		{ "plains", "res://Scenes/Biomes/Plains.tscn" },
		{ "forest", "res://Scenes/Biomes/Forest.tscn" }
	};

	private string currentBiome = "wilderness"; // Default starting biome
	private Node MapDisplay; // ✅ Declare MapDisplay as a class variable




	public override void _Ready()
{
	if (_instance == null)
	{
		_instance = this;
		GD.Print("✅ SceneManager Singleton Initialized.");
	}
	else
	{
		GD.PrintErr("⚠️ Duplicate SceneManager detected! Removing...");
		QueueFree();
		return;
	}

	// ✅ SceneManager is DISABLED until explicitly enabled
	SetProcess(false);
	GD.Print("⏳ SceneManager is DISABLED until manually activated.");
}




// ✅ This method will be called when we want to enable SceneManager
public void EnableSceneManager()
{
	GD.Print("✅ SceneManager Activated!");

	// ✅ Ensure World is Ready BEFORE calling this function
	if (World.Instance == null || !World.Instance.IsReady)
	{
		GD.PrintErr("❌ ERROR: World is not ready! SceneManager should not have been activated yet.");
		return;
	}

	GD.Print("✅ World is ready! Now loading the correct biome...");
	LoadCurrentBiome();
}





public async void LoadCurrentBiome()
{
	GD.Print("🌍 SceneManager: LoadCurrentBiome() started.");

	int attempts = 0;
	while (!World.biomeMapReady && attempts < 200)
	{
		GD.Print($"⏳ Waiting for World to finish loading... ({attempts + 1}/200)");
		await Task.Delay(500);
		attempts++;
	}

	if (!World.biomeMapReady)
	{
		GD.PrintErr("❌ World failed to generate in time! Defaulting to Wilderness.");
		GetTree().ChangeSceneToFile("res://Scenes/Biomes/Wilderness.tscn");
		return;
	}

	string biomeType = World.GetBiomeAt(World.playerTile);
	GD.Print($"🌍 SceneManager: Determined biome at player tile {World.playerTile}: {biomeType}");

	if (biomeType == "Unknown" || !biomeScenes.ContainsKey(biomeType))
	{
		GD.PrintErr($"❌ Invalid biome detected: {biomeType}. Defaulting to Wilderness.");
		biomeType = "wilderness"; // ✅ Force Wilderness as fallback
	}

	PackedScene biomeScene = (PackedScene)ResourceLoader.Load(biomeScenes[biomeType]);
	if (biomeScene == null)
	{
		GD.PrintErr($"❌ Failed to load scene for biome: {biomeType}");
		return;
	}

	Node MapDisplay = GetTree().Root.GetNodeOrNull("UI/MapDisplay");
	if (MapDisplay == null)
	{
		GD.PrintErr("❌ MapDisplay not found! Cannot attach biome.");
		return;
	}

	// ✅ Clear any existing biome first
	foreach (Node child in MapDisplay.GetChildren())
	{
		MapDisplay.RemoveChild(child);
		child.QueueFree();
	}

	// ✅ Add the new biome scene
	Node biomeInstance = biomeScene.Instantiate();
	MapDisplay.AddChild(biomeInstance);
	GD.Print($"✅ Biome {biomeType} added to MapDisplay.");

	// ZoneCreation Trigger
	if (MapDisplay.HasNode("ZoneCreation") == false)
{
	ZoneCreation zone = new ZoneCreation();
	MapDisplay.AddChild(zone);
	GD.Print("🌍 ZoneCreation successfully added to the scene!");
}


	// ✅ Now, add the player after the map is set up
	AddPlayerToMapDisplay(MapDisplay);
}

private void AddPlayerToMapDisplay(Node MapDisplay)
{
	PackedScene playerScene = (PackedScene)ResourceLoader.Load("res://Scenes/Sprites/PlayerSprites/character.tscn");
	if (playerScene == null)
	{
		GD.PrintErr("❌ Failed to load Player.tscn");
		return;
	}

	Node playerInstance = playerScene.Instantiate();
	MapDisplay.AddChild(playerInstance);
	GD.Print("✅ Player added to MapDisplay!");

	// Ensure player starts at correct tile position
	if (playerInstance is Node2D player2D)
	{
		Vector2 playerPosition = World.GetTilePosition(World.playerTile);
		player2D.Position = playerPosition;
		GD.Print($"🚀 Player positioned at {playerPosition}");
	}
}



}
