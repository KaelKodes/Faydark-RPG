using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
	private bool isSceneManagerEnabled = false; // ✅ Prevents double execution
	private static SceneManager _instance;
	public static SceneManager Instance => _instance ??= new SceneManager();

	private Dictionary<string, string> biomeScenes = new Dictionary<string, string>
	{
		{ "wilderness", "res://Scenes/Biomes/Wilderness.tscn" },
		{ "plains", "res://Scenes/Biomes/Plains.tscn" },
		{ "forest", "res://Scenes/Biomes/Forest.tscn" }
	};

	private string currentBiome = "wilderness"; // Default biome
	private Node MapDisplay; // ✅ Store reference to MapDisplay

	public override void _Ready()
	{
		if (_instance == null)
		{
			_instance = this;
			GD.Print("✅ SceneManager Initialized.");
		}
		else
		{
			GD.PrintErr("❌ Duplicate SceneManager detected!");
			QueueFree();
		}
	}

	// ✅ Enables SceneManager but prevents duplicate activation
	public void EnableSceneManager()
	{
		if (isSceneManagerEnabled)
		{
			GD.Print("⚠️ SceneManager is already enabled. Skipping duplicate call.");
			return;
		}

		isSceneManagerEnabled = true;
		GD.Print("✅ SceneManager Activated!");

		Node gameScene = GetTree().Root.GetNodeOrNull("GameScene");
		if (gameScene == null)
		{
			GD.PrintErr("❌ ERROR: `GameScene` not found in root!");
			return;
		}

		MapDisplay = GetTree().Root.GetNodeOrNull("UI/MapDisplay");
		if (MapDisplay == null)
		{
			GD.PrintErr("❌ ERROR: `MapDisplay` not found!");
			return;
		}

		GD.Print("🔍 SceneManager ready to handle zones.");
	}



	// ✅ Loads a new zone only if player crosses a zone boundary
	public async void LoadCurrentZone()
{
	GD.Print("🌍 SceneManager: LoadCurrentZone() started.");

	// ✅ Determine which zone the player is in (NOT just the tile)
	Vector2I currentZone = World.GetZoneForTile(World.playerTile);

	// 🛑 **Prevent Null Zone Access**
	if (currentZone == null)
	{
		GD.PrintErr("❌ ERROR: Player is in an invalid zone! Cannot load.");
		return;
	}

	// ✅ Check if player is in the same zone before reloading
	if (GameState.LastZone == currentZone)
	{
		GD.Print("🚀 Player is still in the same zone. No need to reload.");
		return;
	}

	GD.Print($"🌍 SceneManager: Loading Zone {currentZone}");

	// ✅ Declare `existingZone` once at the start
	ZoneCreation existingZone = GameState.GetZone(currentZone);
	string biomeType = (existingZone != null) ? existingZone.BiomeType : "wilderness";

	if (string.IsNullOrEmpty(biomeType))
{
	GD.PrintErr($"❌ ERROR: Zone data missing for {currentZone}. Defaulting to wilderness.");
	biomeType = "wilderness";
}

Node mapDisplay = GetTree().Root.GetNodeOrNull("UI/MapDisplay");
if (mapDisplay == null)
{
	GD.PrintErr("❌ ERROR: MapDisplay not found!");
	return;
}

	// ✅ Check if the zone already exists before creating a new one
if (GameState.ZoneExists(currentZone))
{
	GD.Print($"✅ Loading previously generated zone for {currentZone}...");

	if (existingZone == null || existingZone.IsQueuedForDeletion() || !IsInstanceValid(existingZone))
	{
		GD.PrintErr($"❌ ERROR: Zone {currentZone} is invalid. Recreating it...");
		GameState.RemoveZone(currentZone);

		existingZone = new ZoneCreation(); // ✅ Just assign, no redeclaration
		GameState.SaveZone(currentZone, existingZone); // ✅ Pass the dictionary inside `ZoneCreation`
	}

	if (!mapDisplay.HasNode(existingZone.Name.ToString())) // ✅ Convert `StringName` to string
	{
		mapDisplay.AddChild(existingZone);
	}
}
else
{
	GD.Print($"🌍 No ZoneCreation found for {currentZone}. Creating a new one...");
	ZoneCreation newZone = new ZoneCreation();
	mapDisplay.AddChild(newZone);
	newZone.GenerateZoneForBiome(biomeType);
	GameState.SaveZone(currentZone, newZone); // ✅ Correct dictionary type
}

// ✅ Update Last Zone
GameState.LastZone = currentZone;
}


	// ✅ Handles player spawning in the correct zone
	private void SpawnPlayer()
	{
		if (!GameState.PlayerExists())
		{
			GD.PrintErr("❌ Player data is missing!");
			return;
		}

		Node2D player = GameState.LoadPlayer();
		if (!MapDisplay.HasNode("Player"))
		{
			MapDisplay.AddChild(player);
			GD.Print("✅ Player added to MapDisplay!");
		}

		// Ensure player is positioned correctly
		Vector2 playerPosition = World.GetTilePosition(World.playerTile);
		player.Position = playerPosition;
		GD.Print($"🚀 Player positioned at {playerPosition}");
	}
}
