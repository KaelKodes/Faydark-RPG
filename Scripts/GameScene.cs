using Godot;
using System;


public partial class GameScene : Node2D  // ✅ Ensure GameScene inherits from Node2D
{
	//private Node MapDisplay;
	private Node2D MapDisplay;

	public override void _Ready()
{
	GD.Print("⏳ GameScene is DISABLED until enabled.");
	SetProcess(false);

	MapDisplay = GetNodeOrNull<Node2D>("MapDisplay");
	if (MapDisplay == null)
	{
		GD.PrintErr("❌ ERROR: `MapDisplay` is NULL inside GameScene!");
	}
	else
	{
		GD.Print("✅ `MapDisplay` successfully found in GameScene!");
	}
}


// ✅ This method will be called when we want to enable GameScene
public void EnableGameScene()
{
	GD.Print("🎬 GameScene is now ACTIVE!");
	SetProcess(true);
	LoadStartingMap();
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
	SceneManager.Instance.LoadCurrentBiome();
}


}
