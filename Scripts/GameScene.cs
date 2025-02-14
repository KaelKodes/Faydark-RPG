using Godot;
using System;
using System.Threading.Tasks;


public partial class GameScene : Node2D  // ✅ Ensure GameScene inherits from Node2D
{
	//private Node MapDisplay;
	private Node2D MapDisplay;

	public override void _Ready()
{
	GD.Print("✅ GameScene Loaded and Waiting for Activation.");
}


private bool isGameSceneEnabled = false; // ✅ Prevents double execution
// ✅ This method will be called when we want to enable GameScene
public async void EnableGameScene()
{
	if (isGameSceneEnabled)
	{
		GD.Print("⚠️ GameScene is already enabled. Skipping duplicate call.");
		return;
	}

	isGameSceneEnabled = true; // ✅ Now marked as enabled

	GD.Print("✅ Enabling GameScene...");

	while (ZoneCreation.Instance == null)
	{
		GD.Print("⏳ Waiting for ZoneCreation instance...");
		await Task.Delay(100);
	}

	GD.Print("✅ ZoneCreation instance found! Connecting signal...");

	if (!ZoneCreation.Instance.IsConnected("ZoneRendered", new Callable(this, nameof(OnZoneRendered))))
	{
		ZoneCreation.Instance.Connect("ZoneRendered", new Callable(this, nameof(OnZoneRendered)));
	}
}

private int sceneManagerRetryCount = 0;
private const int sceneManagerMaxRetries = 10;

private async void OnZoneRendered()
{
	GD.Print("✅ Zone rendering complete! Now enabling SceneManager...");

	if (SceneManager.Instance == null)
	{
		if (sceneManagerRetryCount >= sceneManagerMaxRetries)
		{
			GD.PrintErr("❌ ERROR: SceneManager.Instance is STILL NULL after multiple attempts! Aborting.");
			return;
		}

		GD.PrintErr($"❌ ERROR: SceneManager.Instance is NULL! Retrying... Attempt {sceneManagerRetryCount + 1}/{sceneManagerMaxRetries}");
		sceneManagerRetryCount++;
		await Task.Delay(100); // Small delay before retrying
		OnZoneRendered(); // Retry
		return;
	}

	SceneManager.Instance.EnableSceneManager();

	// ✅ Ensure CharacterData exists before spawning player
	if (CharacterData.Instance == null)
	{
		GD.PrintErr("❌ ERROR: CharacterData is NULL! Cannot load player.");
		return;
	}

	SpawnPlayer();
}


	private async void LoadStartingMap()
{
	int attempts = 0;
	while ((SceneManager.Instance == null || World.Instance == null || !World.Instance.IsReady) && attempts < 50)
	{
		GD.Print($"⏳ Waiting for SceneManager & World Generation... ({attempts + 1}/50)");
		await ToSignal(GetTree(), "process_frame");
		attempts++;
	}

	if (SceneManager.Instance == null || World.Instance == null || !World.Instance.IsReady)
	{
		GD.PrintErr("❌ World failed to fully generate! Loading default Wilderness.");
		return;
	}

	GD.Print("✅ World is ready! Now loading the correct biome...");
	SceneManager.Instance.LoadCurrentZone();
}


private void SpawnPlayer()
{
	GD.Print("📌 Spawning Player Character...");

	// ✅ Now we KNOW where MapDisplay is!
	Node mapDisplay = GetTree().Root.FindChild("MapDisplay", true, false);

	if (mapDisplay == null)
	{
		GD.PrintErr("❌ ERROR: `MapDisplay` still not found via GetTree()!");
		PrintSceneTree();
		return;
	}

	GD.Print($"✅ MapDisplay found: {mapDisplay.GetPath()}");

	// ✅ Instantiate player and attach to MapDisplay
	PackedScene playerScene = (PackedScene)ResourceLoader.Load("res://Scenes/Sprites/PlayerSprites/character.tscn");
	Node2D playerInstance = (Node2D)playerScene.Instantiate();
	mapDisplay.AddChild(playerInstance);

	GD.Print("🎮 Player Character successfully spawned inside MapDisplay!");
}

private void PrintSceneTree()
{
	GD.Print("📌 SCENE TREE DUMP:");
	PrintNodeHierarchy(GetTree().Root, 0);
}

private void PrintNodeHierarchy(Node node, int depth)
{
	GD.Print(new string(' ', depth * 2) + $"🔍 {node.Name} ({node.GetType()})");
	foreach (Node child in node.GetChildren())
	{
		PrintNodeHierarchy(child, depth + 1);
	}
}





}
